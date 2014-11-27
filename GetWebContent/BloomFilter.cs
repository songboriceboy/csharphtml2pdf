using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
namespace GetWebContent
{

        public class BloomFilter
        {
            /// <summary>  
            /// BitArray用来替代内存块，在C/C++中可使用BITMAP替代  
            /// </summary>  
            private static BitArray bitArray = null;

            private int size = -1;

            /// <summary>  
            /// 构造函数，初始化分配内存  
            /// </summary>  
            /// <param name="size">分配的内存大小,必须保证被2整除</param>  
            public BloomFilter(int size)
            {
                if (size % 2 == 0)
                {
                    bitArray = new BitArray(size, false);
                    this.size = size;
                }
                else
                {
                    throw new Exception("错误的长度,不能被2整除");
                }
            }

            /// <summary>  
            /// 将str加入Bloomfilter，主要是HASH后找到指定位置置true  
            /// </summary>  
            /// <param name="str">字符串</param>  
            public void Add(string str)
            {
                int[] offsetList = getOffset(str);
                if (offsetList != null)
                {
                    put(offsetList[0]);
                    put(offsetList[1]);
                }
                else
                {
                    throw new Exception("字符串不能为空");
                }
            }

            /// <summary>  
            /// 判断该字符串是否重复  
            /// </summary>  
            /// <param name="str">字符串</param>  
            /// <returns>true重复反之则false</returns>  
            public Boolean Contains(string str)
            {
                int[] offsetList = getOffset(str);
                if (offsetList != null)
                {
                    if ((get(offsetList[0]) == true) && (get(offsetList[1]) == true))
                    {
                        return true;
                    }

                }
                return false;
            }

            /// <summary>  
            /// 返回内存块指定位置状态  
            /// </summary>  
            /// <param name="offset">位置</param>  
            /// <returns>状态为TRUE还是FALSE 为TRUE则被占用</returns>  
            private Boolean get(int offset)
            {
                return bitArray[offset];
            }

            /// <summary>  
            /// 改变指定位置状态  
            /// </summary>  
            /// <param name="offset">位置</param>  
            /// <returns>改变成功返回TRUE否则返回FALSE</returns>  
            private Boolean put(int offset)
            {
                //try  
                //{  
                if (bitArray[offset])
                {
                    return false;
                }
                bitArray[offset] = true;
                //}  
                //catch (Exception e)  
                //{  
                // Console.WriteLine(offset);  
                //}  
                return true;
            }

            public int[] getOffset(string str)
            {
                if (String.IsNullOrEmpty(str) != true)
                {
                    int[] offsetList = new int[2];
                    string tmpCode = Hash(str).ToString();
                    int hashCode = Hash2(tmpCode);
                    int offset = Math.Abs(hashCode % (size / 2) - 1);
                    offsetList[0] = offset;
                    hashCode = Hash3(str);
                    offset = size - Math.Abs(hashCode % (size / 2)) - 1;
                    offsetList[1] = offset;
                    return offsetList;
                }
                return null;
            }
            /// <summary>  
            /// 内存块大小  
            /// </summary>  
            public int Size
            {
                get { return size; }
            }

            /// <summary>  
            /// 获取字符串HASHCODE  
            /// </summary>  
            /// <param name="val">字符串</param>  
            /// <returns>HASHCODE</returns>  
            private int Hash(string val)
            {
                return val.GetHashCode();
            }

            /// <summary>  
            /// 获取字符串HASHCODE  
            /// </summary>  
            /// <param name="val">字符串</param>  
            /// <returns>HASHCODE</returns>  
            private int Hash2(string val)
            {
                int hash = 0;

                for (int i = 0; i < val.Length; i++)
                {
                    hash += val[i];
                    hash += (hash << 10);
                    hash ^= (hash >> 6);
                }
                hash += (hash << 3);
                hash ^= (hash >> 11);
                hash += (hash << 15);
                return hash;
            }

            /// <summary>  
            /// 获取字符串HASHCODE  
            /// </summary>  
            /// <param name="val">字符串</param>  
            /// <returns>HASHCODE</returns>  
            private int Hash3(string str)
            {
                long hash = 0;

                for (int i = 0; i < str.Length; i++)
                {
                    if ((i & 1) == 0)
                    {
                        hash ^= ((hash << 7) ^ str[i] ^ (hash >> 3));
                    }
                    else
                    {
                        hash ^= (~((hash << 11) ^ str[i] ^ (hash >> 5)));
                    }
                }
                unchecked
                {
                    return (int)hash;
                }
            }

        }  
        //static void Main(string[] args)
        //{
        //    System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        //    BloomFilter bf = new BloomFilter(10485760);
        //    const int maxNum = 100000;
        //    double count = 0;

        //    watch.Reset();
        //    watch.Start();
        //    for (int i = 0; i < maxNum; i++)
        //    {
        //        if (bf.Contains(i.ToString()) != true)
        //        {
        //            bf.Add(i.ToString());
        //        }
        //        else
        //        {
        //            //Console.Write("发生碰撞数字:");  
        //            //Console.WriteLine(i);  
        //            count++;
        //        }
        //    }
        //    watch.Stop();
        //    Console.WriteLine("碰撞概率:" + (count / (double)maxNum * 100) + "%");
        //    Console.Write("运行时间:");
        //    Console.WriteLine(watch.ElapsedMilliseconds.ToString() + "ms");  
        //}
 
}
