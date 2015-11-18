using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// ChunkHelper 帮助将序列分成若干块。
    /// 
    /// 如果 totalItemCount 是 11，chunkSize 是 5，则：
    /// chunk[0] 0, 1, 2, 3, 4
    /// chunk[1] 5, 6, 7, 8, 9
    /// chunk[2] 10 
    /// </summary>
    public class ChunkHelper
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="totalItemCount">项目总数</param>
        /// <param name="chunkSize">块大小</param>
        public ChunkHelper(int totalItemCount, int chunkSize)
        {
            if (totalItemCount < 0)
            {
                throw new ArgumentOutOfRangeException("itemCount", totalItemCount, "itemCount 不能小于 0");
            }
            if (chunkSize < 1)
            {
                throw new ArgumentOutOfRangeException("chunkSize", chunkSize, "chunkSize 不能小于 1");
            }
            this.TotalItemCount = totalItemCount;
            this.ChunkSize = chunkSize;

            int i = this.TotalItemCount % this.ChunkSize == 0 ? 0 : 1;
            this.ChunkCount = this.TotalItemCount / this.ChunkSize + i;

        }

        /// <summary>
        /// 获取项目总数。
        /// </summary>
        public int TotalItemCount { get; private set; }

        /// <summary>
        /// 获取块大小。
        /// </summary>
        public int ChunkSize { get; private set; }


        /// <summary>
        /// 指定项目索引，获取此项目所属的分块索引。
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <returns></returns>
        public int GetChunkIndex(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= TotalItemCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            return itemIndex / ChunkSize;
        }

        /// <summary>
        /// 获取分块数量，如果 TotalItemCount 为 0， 则返回值为 0，而不是 1。
        /// </summary>
        public int ChunkCount { get; private set; }


        /// <summary>
        /// 获取指定的分块信息。
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <returns></returns>
        public Chunk GetChunk(int chunkIndex)
        {
            if (chunkIndex < 0 || chunkIndex >= ChunkCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            int startItemIndex = chunkIndex * ChunkSize;
            int itemCountInChunk = ChunkSize;

            //  如果大于项目总数，则是最后一块
            if (startItemIndex + itemCountInChunk > TotalItemCount)
            {
                itemCountInChunk = TotalItemCount % ChunkSize;
            }
            return new Chunk(chunkIndex, startItemIndex, itemCountInChunk);
        }

    }


    /// <summary>
    /// Chunk 表示一个分块。
    /// </summary>
    public struct Chunk
    {
        /// <summary>
        /// 本块的索引。
        /// </summary>
        public int ChunkIndex { get; private set; }

        /// <summary>
        /// 本块第一个项目在整个序列中的索引。
        /// </summary>
        public int FirstItemIndex { get; private set; }

        /// <summary>
        /// 本块的项目数。
        /// </summary>
        public int ItemCount { get; private set; }

        /// <summary>
        /// 实例化 Chunk
        /// </summary>
        /// <param name="chunkIndex"></param>
        /// <param name="firstItemIndex"></param>
        /// <param name="itemCountInChunk"></param>
        internal Chunk(int chunkIndex, int firstItemIndex, int itemCountInChunk)
            : this()
        {
            this.ChunkIndex = chunkIndex;
            this.FirstItemIndex = firstItemIndex;
            this.ItemCount = itemCountInChunk;
        }
    }
}