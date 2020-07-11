using System.Collections.Generic;
using System.Linq;

namespace LifeGame.Models
{
    /// <summary>
    /// セルの状態を表すクラスを定義します。
    /// </summary>
    public class Cells
    {
        /// <summary>
        /// セルの状態を取得します。
        /// </summary>
        public bool[] Conditions { get; private set; }

        /// <summary>
        /// 一次元配列上における8方向のインデックスの差分を格納するコレクションを定義します。
        /// </summary>
        private readonly int[] directions = new int[8];

        /// <summary>
        /// <see cref="Cells"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="h">セルの横の個数を設定します。</param>
        /// <param name="v">セルの縦の個数を設定します。</param>
        public Cells(int h, int v)
        {
            // セルの常置を初期化
            Conditions = Enumerable.Repeat(false, h * v).ToArray();

            // 各方向の移動量を設定
            directions[0] = -h - 1;
            directions[1] = -h;
            directions[2] = -h + 1;
            directions[3] = -1;
            directions[4] = 1;
            directions[5] = h - 1;
            directions[6] = h;
            directions[7] = h + 1;

            // セルに初期値を設定
            Conditions[0 * h + 1] = true;
            Conditions[1 * h + 2] = true;
            Conditions[2 * h + 0] = true;
            Conditions[2 * h + 1] = true;
            Conditions[2 * h + 2] = true;
        }

        /// <summary>
        /// <see cref="Conditions"/>プロパティで表されるセルの世代を一つ進めます。
        /// </summary>
        public void CalcNextGeneration()
        {
            var results = new bool[Conditions.Length];

            foreach (var (count, index) in GetExistCellCounts().Select((value, index) => (value, index)))
            {
                if (!Conditions[index])
                {
                    results[index] = count == 3;
                }
                else if (Conditions[index])
                {
                    results[index] = count == 2 || count == 3;
                }
            }

            for (var i = 0; i < results.Length; i++)
            {
                Conditions[i] = results[i];
            }
        }

        /// <summary>
        /// セルの周囲の生存セルをカウントします。
        /// </summary>
        /// <returns>各セルの周囲の生存セルの個数を格納する列挙可能なコレクションを返します。</returns>
        private IEnumerable<int> GetExistCellCounts()
        {
            for (var i = 0; i < Conditions.Length; i++)
            {
                var count = 0;

                foreach (var dir in directions)
                {
                    if (i + dir < 0)
                    {
                        if (Conditions[i + dir + Conditions.Length])
                        {
                            count++;
                        }
                    }
                    else if (i + dir >= Conditions.Length)
                    {
                        if (Conditions[i + dir - Conditions.Length])
                        {
                            count++;
                        }
                    }
                    else if (Conditions[i + dir])
                    {
                        count++;
                    }
                }

                yield return count;
            }
        }
    }
}
