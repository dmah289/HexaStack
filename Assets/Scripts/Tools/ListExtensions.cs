using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    // Không thể sử dụng ref do truy cập list theo index là truy cập bản sao nên phải thay đổi trực tiếp vào list
    private static void Swap<T>(List<T> list, int idx1, int idx2)
    {
        T temp = list[idx1];
        list[idx1] = list[idx2];
        list[idx2] = temp;
    }

    private static int HoarePartition<T>(List<T> list, int l ,int r, IComparer<T> comparer)
    {
        T pivot = list[l];
        int i = l - 1, j = r + 1;

        while(true)
        {
            do { i++; }
            while (comparer.Compare(list[i], pivot) < 0);

            do { j--; }
            while (comparer.Compare(list[j], pivot) > 0);

            if (i < j) Swap(list, i, j);
            else return j;
        }
    }
    
    public static void QuickSortHoare<T>(this List<T> list, int l, int r, IComparer<T> comparer)
    {
        if (l >= r) return;

        int pivot = HoarePartition<T>(list, l, r, comparer);
        QuickSortHoare(list, l, pivot, comparer);
        QuickSortHoare(list, pivot + 1, r, comparer);
    }

    public static bool UniqueRow<T> (this List<T> list) where T : GridCell
    {
        for(int i = 0; i < list.Count-1; i++)
        {
            if (list[i].row == list[i + 1].row)
                return false;
        }
        return true;
    }
}
