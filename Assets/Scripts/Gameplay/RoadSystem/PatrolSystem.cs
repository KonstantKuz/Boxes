using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.RoadSystem
{
    public class PatrolSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RoadSystem roadSystem;

        [Header("Cycle Detection Settings")]
        [SerializeField] private int minCycleLength = 4;
        [SerializeField] private int maxCycleLength = 20;

        [Header("Car Distribution Settings")]
        [SerializeField] private float carsPerUnitLength = 0.05f; // машин на единицу длины
        [SerializeField] private int minCarsPerCycle = 1;
        [SerializeField] private int maxCarsPerCycle = 5;
        [SerializeField] private float speedVariation = 0.15f; // вариация скорости (±15%)
        [SerializeField] private bool teleportCarsToStart = true; // телепортировать машины на старт

        private PatrolCar[] patrolCars;
        private List<List<RoadSystem.Node>> detectedCycles;
        private bool routesAssigned = false;

        private void Awake()
        {
            patrolCars = GetComponentsInChildren<PatrolCar>();
            foreach (PatrolCar car in patrolCars)
            {
                car.SetSystem(this);
            }

            // Вычисляем и назначаем маршруты один раз при старте
            DetectCyclesAndAssignCars();
        }

        public void SetPatrolActive(bool value)
        {
            // Если маршруты ещё не назначены, назначаем их
            if (value && !routesAssigned)
            {
                DetectCyclesAndAssignCars();
            }

            // Просто включаем/выключаем движение без пересчёта маршрутов
            foreach (PatrolCar car in patrolCars)
            {
                car.SetIsDriving(value);
            }
        }

        private void DetectCyclesAndAssignCars()
        {
            if (routesAssigned)
            {
                Debug.Log("PatrolSystem: Routes already assigned, skipping recalculation");
                return;
            }

            if (roadSystem == null || roadSystem.Nodes.Count < minCycleLength)
            {
                Debug.LogWarning("PatrolSystem: Not enough nodes to detect cycles");
                return;
            }

            Debug.Log("PatrolSystem: Detecting cycles and assigning routes...");

            detectedCycles = roadSystem.FindAllCycles(minCycleLength, maxCycleLength);

            if (detectedCycles == null || detectedCycles.Count == 0)
            {
                Debug.LogWarning("PatrolSystem: No cycles found in road system");
                return;
            }

            Debug.Log($"PatrolSystem: Found {detectedCycles.Count} cycles");

            AssignCarsToCycles();

            routesAssigned = true;
            Debug.Log("PatrolSystem: Routes assigned successfully");
        }

        private void AssignCarsToCycles()
        {
            int carCount = patrolCars.Length;
            int cycleCount = detectedCycles.Count;

            // Вычисляем длину каждого цикла
            List<(int index, float length)> cyclesWithLengths = new List<(int, float)>();
            float totalLength = 0f;

            for (int i = 0; i < cycleCount; i++)
            {
                float length = roadSystem.CalculateCycleLength(detectedCycles[i]);
                cyclesWithLengths.Add((i, length));
                totalLength += length;
            }

            // Вычисляем желаемое количество машин для каждого цикла пропорционально его длине
            Dictionary<int, int> desiredCarsPerCycle = new Dictionary<int, int>();
            int totalAssignedCars = 0;

            foreach (var (index, length) in cyclesWithLengths)
            {
                float proportion = length / totalLength;
                int desiredCars = Mathf.Clamp(
                    Mathf.RoundToInt(proportion * carCount),
                    minCarsPerCycle,
                    maxCarsPerCycle
                );

                desiredCarsPerCycle[index] = desiredCars;
                totalAssignedCars += desiredCars;
            }

            // Корректируем, если машин не хватает или слишком много
            if (totalAssignedCars != carCount)
            {
                AdjustCarDistribution(desiredCarsPerCycle, cyclesWithLengths, carCount, totalAssignedCars);
            }

            // Распределяем машины по циклам
            Dictionary<int, List<CarAgent>> cycleAssignments = new Dictionary<int, List<CarAgent>>();
            for (int i = 0; i < cycleCount; i++)
            {
                cycleAssignments[i] = new List<CarAgent>();
            }

            int carIndex = 0;
            foreach (var kvp in desiredCarsPerCycle)
            {
                int cycleIndex = kvp.Key;
                int carsForCycle = kvp.Value;

                for (int i = 0; i < carsForCycle && carIndex < carCount; i++)
                {
                    CarAgent agent = patrolCars[carIndex].GetComponent<CarAgent>();
                    if (agent != null)
                    {
                        cycleAssignments[cycleIndex].Add(agent);
                    }
                    carIndex++;
                }
            }

            foreach (var kvp in cycleAssignments)
            {
                int cycleIndex = kvp.Key;
                List<CarAgent> carsOnCycle = kvp.Value;

                if (carsOnCycle.Count == 0) continue;

                List<RoadSystem.Node> cycle = detectedCycles[cycleIndex];
                float cycleLength = roadSystem.CalculateCycleLength(cycle);

                // Определяем направление цикла по часовой стрелке
                List<RoadSystem.Node> clockwiseCycle = EnsureClockwiseOrder(cycle);

                // Вычисляем среднюю скорость машин на этом цикле
                float averageSpeed = CalculateAverageSpeed(carsOnCycle);

                // Время полного круга для одной машины
                float cycleTime = cycleLength / averageSpeed;

                // Временной интервал между машинами для равномерного распределения
                float timeInterval = cycleTime / carsOnCycle.Count;

                // Нормализуем скорости машин на этом цикле для лучшего распределения
                NormalizeCarSpeeds(carsOnCycle, averageSpeed);

                for (int idx = 0; idx < carsOnCycle.Count; idx++)
                {
                    CarAgent agent = carsOnCycle[idx];

                    // Вычисляем стартовую позицию с учётом времени
                    // Используем среднюю скорость для равномерного временного распределения
                    float timeOffset = timeInterval * idx;
                    float startOffset = (averageSpeed * timeOffset) % cycleLength;

                    // Получаем точную позицию на цикле с учётом offset
                    (int startNodeIndex, Vector3 exactStartPosition) = GetPositionAtDistance(clockwiseCycle, startOffset);

                    // Создаём маршрут начиная с стартовой ноды, всегда по часовой
                    List<RoadSystem.Node> orderedCycle = new List<RoadSystem.Node>();
                    for (int i = 0; i < clockwiseCycle.Count; i++)
                    {
                        int index = (startNodeIndex + i) % clockwiseCycle.Count;
                        orderedCycle.Add(clockwiseCycle[index]);
                    }

                    // Сначала назначаем маршрут
                    agent.SetAssignedRoute(orderedCycle);

                    // Затем телепортируем машину на точную стартовую позицию (ДО запуска движения)
                    if (teleportCarsToStart)
                    {
                        agent.transform.position = exactStartPosition;

                        // Также поворачиваем машину в направлении движения
                        RoadSystem.Node currentNode = clockwiseCycle[startNodeIndex];
                        RoadSystem.Node nextNode = clockwiseCycle[(startNodeIndex + 1) % clockwiseCycle.Count];
                        Vector3 direction = (nextNode.Position - currentNode.Position).normalized;

                        if (direction.sqrMagnitude > 0.001f)
                        {
                            agent.transform.rotation = Quaternion.LookRotation(direction);
                        }

                        Debug.Log($"Teleported car {idx} to exact start position at {exactStartPosition}");
                    }

                    Debug.Log($"Assigned car {idx} to cycle {cycleIndex} (length={cycleLength:F1}), " +
                              $"start offset={startOffset:F1}m, speed={agent.speed:F1}, direction=CW");
                }
            }
        }

        private (int nodeIndex, Vector3 position) GetPositionAtDistance(List<RoadSystem.Node> cycle, float targetDistance)
        {
            float accumulatedDistance = 0f;

            for (int i = 0; i < cycle.Count; i++)
            {
                RoadSystem.Node current = cycle[i];
                RoadSystem.Node next = cycle[(i + 1) % cycle.Count];

                float edgeLength = 0f;
                foreach (RoadSystem.Edge edge in current.Edges)
                {
                    if (edge.GetOther(current) == next)
                    {
                        edgeLength = edge.Length;
                        break;
                    }
                }

                if (accumulatedDistance + edgeLength >= targetDistance)
                {
                    // Вычисляем точную позицию между нодами
                    float distanceOnEdge = targetDistance - accumulatedDistance;
                    float t = edgeLength > 0.001f ? distanceOnEdge / edgeLength : 0f;
                    Vector3 exactPosition = Vector3.Lerp(current.Position, next.Position, t);

                    return (i, exactPosition);
                }

                accumulatedDistance += edgeLength;
            }

            // Если не нашли, возвращаем первую ноду
            return (0, cycle[0].Position);
        }

        private float CalculateAverageSpeed(List<CarAgent> agents)
        {
            if (agents.Count == 0) return 5f;

            float totalSpeed = 0f;
            foreach (CarAgent agent in agents)
            {
                totalSpeed += agent.speed;
            }

            return totalSpeed / agents.Count;
        }

        private void NormalizeCarSpeeds(List<CarAgent> agents, float targetSpeed)
        {
            if (agents.Count == 0) return;

            // Немного варьируем скорости машин вокруг целевой скорости
            // Это предотвращает полное скучивание при разных начальных скоростях
            for (int i = 0; i < agents.Count; i++)
            {
                // Каждая машина получает немного отличающуюся скорость
                // Вариация зависит от её позиции в списке для предсказуемости
                float speedVariationFactor = 1f + speedVariation * Mathf.Sin((float)i / agents.Count * Mathf.PI * 2);
                agents[i].speed = targetSpeed * speedVariationFactor;
            }
        }

        private List<RoadSystem.Node> EnsureClockwiseOrder(List<RoadSystem.Node> cycle)
        {
            if (cycle.Count < 3)
            {
                return cycle;
            }

            // Вычисляем площадь многоугольника с помощью формулы Shoelace
            // Если площадь положительная - обход против часовой, если отрицательная - по часовой
            float signedArea = 0f;
            for (int i = 0; i < cycle.Count; i++)
            {
                Vector3 current = cycle[i].Position;
                Vector3 next = cycle[(i + 1) % cycle.Count].Position;
                signedArea += (next.x - current.x) * (next.z + current.z);
            }

            // Если площадь положительная (против часовой), переворачиваем список
            if (signedArea > 0)
            {
                List<RoadSystem.Node> reversed = new List<RoadSystem.Node>(cycle);
                reversed.Reverse();
                return reversed;
            }

            return cycle;
        }

        private void AdjustCarDistribution(Dictionary<int, int> desiredCarsPerCycle,
            List<(int index, float length)> cyclesWithLengths, int totalCars, int currentTotal)
        {
            // Сортируем циклы по длине (от большего к меньшему)
            List<(int index, float length)> sortedCycles = cyclesWithLengths
                .OrderByDescending(c => c.length)
                .ToList();

            int diff = totalCars - currentTotal;

            if (diff > 0)
            {
                // Нужно добавить машины - добавляем на самые длинные циклы
                foreach (var (index, length) in sortedCycles)
                {
                    if (diff <= 0) break;

                    if (desiredCarsPerCycle[index] < maxCarsPerCycle)
                    {
                        int canAdd = maxCarsPerCycle - desiredCarsPerCycle[index];
                        int toAdd = Mathf.Min(canAdd, diff);
                        desiredCarsPerCycle[index] += toAdd;
                        diff -= toAdd;
                    }
                }
            }
            else if (diff < 0)
            {
                // Нужно убрать машины - убираем с самых коротких циклов
                foreach (var (index, length) in sortedCycles.AsEnumerable().Reverse())
                {
                    if (diff >= 0) break;

                    if (desiredCarsPerCycle[index] > minCarsPerCycle)
                    {
                        int canRemove = desiredCarsPerCycle[index] - minCarsPerCycle;
                        int toRemove = Mathf.Min(canRemove, -diff);
                        desiredCarsPerCycle[index] -= toRemove;
                        diff += toRemove;
                    }
                }
            }
        }
    }
}
