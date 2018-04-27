using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public struct PoolItem
{
    public USAction.POSITION MainTaskPos;
    public USAction.POSITION SecondaryTaskPos;

    public PoolItem(USAction.POSITION mainTaskPos, USAction.POSITION secondaryTaskPos)
    {
        MainTaskPos = mainTaskPos;
        SecondaryTaskPos = secondaryTaskPos;
    }
}

public class USActionPoolGenerator : MonoBehaviour
{
    private const int ActionPositions = 3;

    private const int SecondaryTaskConditions = 6;
    private const int SecondaryTaskConditionRepetitions = 4;

    private const int MinMainTasksOnlyBeforeSecondaryTask = 2;
    private const int MaxMainTasksOnlyBeforeSecondaryTask = 6;

    private const int TrainingMainTaskRepetitions = 6;
    private const int TrainingSecondaryTaskRepetitions = 3;


    public List<PoolItem> GeneratePool(bool forTraining)
    {
        return forTraining ? GenerateTrainingPool(TrainingMainTaskRepetitions, TrainingSecondaryTaskRepetitions) : GenerateStudyPool();
    }

    public List<PoolItem> GenerateTrainingPool(int mainTaskRepetitions, int secondaryTaskRepetitions)
    {
        List<PoolItem> actionPool = new List<PoolItem>();

        for (int i = 0; i < mainTaskRepetitions; i++)
        {
            int rndPos = Random.Range(0, ActionPositions);
            var poolItem = new PoolItem((USAction.POSITION)rndPos, USAction.POSITION.None);
            actionPool.Add(poolItem);
        }

        for (int i = 0; i < ActionPositions; i++)
        {
            var poolItem = new PoolItem(USAction.POSITION.None, (USAction.POSITION) i);

            for (int j = 0; j < secondaryTaskRepetitions - 1; j++)
            {
                actionPool.Add(poolItem);
            }
        }

        for (int i = 0; i < ActionPositions; i++)
        {
            var poolItem = new PoolItem(USAction.POSITION.None, (USAction.POSITION)i);
            actionPool.Add(poolItem);
        }

        return actionPool;
    }

    public List<PoolItem> GenerateStudyPool()
    {
        List<int> secondaryTaskPool = CreateSecondaryTaskPool();
        var actionPool = new List<PoolItem>();

        int secondaryTaskCounter = MaxMainTasksOnlyBeforeSecondaryTask;

        while (secondaryTaskPool.Count > 0)
        {
            PoolItem poolItem;

            if (secondaryTaskCounter-- == 0)
            {
                poolItem = CreateDoubleTaskPoolItem(ref secondaryTaskPool);
                secondaryTaskCounter = Random.Range(MinMainTasksOnlyBeforeSecondaryTask, MaxMainTasksOnlyBeforeSecondaryTask);
            }
            else
            {
                var rndPos = (USAction.POSITION)Random.Range(0, ActionPositions);
                poolItem = new PoolItem(rndPos, USAction.POSITION.None);
            }

            actionPool.Add(poolItem);
        }
        
        return actionPool;
    }

    private static List<int> CreateSecondaryTaskPool()
    {
        var secondaryTaskPool = new List<int>();

        for (int i = 0; i < SecondaryTaskConditionRepetitions; i++)
        {
            secondaryTaskPool.AddRange(Enumerable.Range(1, SecondaryTaskConditions));
        }

        return secondaryTaskPool;
    }

    private static PoolItem CreateDoubleTaskPoolItem(ref List<int> secondaryTaskPool)
    {
        int secondaryTaskCondition = TakeRandomConditionFromPool(ref secondaryTaskPool);

        var secondaryTaskPosition = (USAction.POSITION)(secondaryTaskCondition / 2);
        var taskRelation = (USAction.RELATION)((secondaryTaskCondition / 2) + 1);

        var mainTaskPosition = secondaryTaskPosition;
        if (taskRelation == USAction.RELATION.Asynchronous)
        {
            mainTaskPosition = GetRandomPositionExcluding(secondaryTaskPosition);
        }

        return new PoolItem(mainTaskPosition, secondaryTaskPosition);
    }

    private static int TakeRandomConditionFromPool(ref List<int> pool)
    {
        int taskIdx = Random.Range(0, pool.Count);

        int condition = pool[taskIdx];
        pool.RemoveAt(taskIdx);

        return condition;
    }

    private static USAction.POSITION GetRandomPositionExcluding(USAction.POSITION posToExclude)
    {
        USAction.POSITION[] remainingPositions = Enum.GetValues(typeof(USAction.POSITION))
            .Cast<USAction.POSITION>()
            .Where(pos => pos != posToExclude && pos != USAction.POSITION.None)
            .ToArray();

        return remainingPositions[Random.Range(0, remainingPositions.Length)];
    }
}
