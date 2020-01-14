using System.Collections;
using System.Collections.Generic;

public static class Utillity {
    public static T[] ShuffleArray<T>(T[] array, int seed) {
        //设置伪随机数生成器的seed
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++) {
            //生成(i, array.Length)之间的随机整数并交换位置
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }
}
