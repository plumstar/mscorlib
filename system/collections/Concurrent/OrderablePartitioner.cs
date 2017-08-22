﻿// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// OrderablePartitioner.cs
//
// <OWNER>[....]</OWNER>
//
// 
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Threading;

namespace System.Collections.Concurrent
{

    /// <summary>
    /// 表示将一个可排序数据源拆分成多个分区的特定方式。
    /// </summary>
    /// <typeparam name="TSource">集合中的元素类型</typeparam>
    /// <remarks>
    /// <para>
    /// Each element in each partition has an integer index associated(关联) with it, which determines(决定) the relative（相对）
    /// order（顺序） of that element against elements in other partitions.
    /// </para>
    /// <para>
    /// Inheritors of <see cref="OrderablePartitioner{TSource}"/> must adhere to the following rules:
    /// <ol>
    /// <li>All indices must be unique, such that there may not be duplicate indices. If all indices are not
    /// unique, the output ordering may be scrambled.</li>
    /// <li>All indices must be non-negative. If any indices are negative, consumers of the implementation
    /// may throw exceptions.</li>
    /// <li><see cref="GetPartitions"/> and <see cref="GetOrderablePartitions"/> should throw a
    /// <see cref="T:System.ArgumentOutOfRangeException"/> if the requested partition count is less than or
    /// equal to zero.</li>
    /// <li><see cref="GetPartitions"/> and <see cref="GetOrderablePartitions"/> should always return a number
    /// of enumerables equal to the requested partition count. If the partitioner runs out of data and cannot
    /// create as many partitions as requested, an empty enumerator should be returned for each of the
    /// remaining partitions. If this rule is not followed, consumers of the implementation may throw a <see
    /// cref="T:System.InvalidOperationException"/>.</li>
    /// <li><see cref="GetPartitions"/>, <see cref="GetOrderablePartitions"/>,
    /// <see cref="GetDynamicPartitions"/>, and <see cref="GetOrderableDynamicPartitions"/>
    /// should never return null. If null is returned, a consumer of the implementation may throw a
    /// <see cref="T:System.InvalidOperationException"/>.</li>
    /// <li><see cref="GetPartitions"/>, <see cref="GetOrderablePartitions"/>,
    /// <see cref="GetDynamicPartitions"/>, and <see cref="GetOrderableDynamicPartitions"/>
    /// should always return partitions that can fully and uniquely enumerate the input data source. All of
    /// the data and only the data contained in the input source should be enumerated, with no duplication
    /// that was not already in the input, unless specifically required by the particular partitioner's
    /// design. If this is not followed, the output ordering may be scrambled.</li>
    /// <li>If <see cref="KeysOrderedInEachPartition"/> returns true, each partition must return elements
    /// with increasing key indices.</li>
    /// <li>If <see cref="KeysOrderedAcrossPartitions"/> returns true, all the keys in partition numbered N
    /// must be larger than all the keys in partition numbered N-1.</li>
    /// <li>If <see cref="KeysNormalized"/> returns true, all indices must be monotonically increasing from
    /// 0, though not necessarily within a single partition.</li>
    /// </ol>
    /// </para>
    /// </remarks>
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    public abstract class OrderablePartitioner<TSource> : Partitioner<TSource>
    {
        /// <summary>
        /// 从派生类中的构造函数进行调用以便使用索引键上指定的约束初始化 System.Collections.Concurrent.OrderablePartitioner<TSource>
        ///     类。
        /// </summary>
        /// <param name="keysOrderedInEachPartition">
        /// 指示是否按键增加的顺序生成每个分区中的元素。
        /// </param>
        /// <param name="keysOrderedAcrossPartitions">
        /// 指示前一分区中的元素是否始终排在后一分区中的元素之前。 如果为 true，则分区 0 中的每个元素的顺序键比分区 1 中的任何元素都要小，分区 1
        ///     中的每个元素的顺序键比分区 2 中的任何元素都要小，依次类推。
        /// </param>
        /// <param name="keysNormalized">
        /// 指示是否规范化键。 如果为 true，所有顺序键均为范围 [0 .. numberOfElements-1] 中的不同整数。 如果为 false，顺序键仍必须互不相同，但只考虑其相对顺序，而不考虑其绝对值。
        /// </param>
        protected OrderablePartitioner(bool keysOrderedInEachPartition, bool keysOrderedAcrossPartitions, bool keysNormalized)
        {
            KeysOrderedInEachPartition = keysOrderedInEachPartition;
            KeysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
            KeysNormalized = keysNormalized;
        }

        /// <summary>
        /// Partitions the underlying collection into the specified number of orderable partitions.
        /// </summary>
        /// <remarks>
        /// Each partition is represented as an enumerator over key-value pairs.
        /// The value of the pair is the element itself, and the key is an integer which determines
        /// the relative ordering of this element against other elements in the data source.
        /// </remarks>
        /// <param name="partitionCount">The number of partitions to create.</param>
        /// <returns>A list containing <paramref name="partitionCount"/> enumerators.</returns>
        public abstract IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount);

        /// <summary>
        /// Creates an object that can partition the underlying collection into a variable number of
        /// partitions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned object implements the <see
        /// cref="T:System.Collections.Generic.IEnumerable{TSource}"/> interface. Calling <see
        /// cref="System.Collections.Generic.IEnumerable{TSource}.GetEnumerator">GetEnumerator</see> on the
        /// object creates another partition over the sequence.
        /// </para>
        /// <para>
        /// Each partition is represented as an enumerator over key-value pairs. The value in the pair is the element
        /// itself, and the key is an integer which determines the relative ordering of this element against
        /// other elements.
        /// </para>
        /// <para>
        /// The <see cref="GetOrderableDynamicPartitions"/> method is only supported if the <see
        /// cref="System.Collections.Concurrent.Partitioner{TSource}.SupportsDynamicPartitions">SupportsDynamicPartitions</see>
        /// property returns true.
        /// </para>
        /// </remarks>
        /// <returns>An object that can create partitions over the underlying data source.</returns>
        /// <exception cref="NotSupportedException">Dynamic partitioning is not supported by this
        /// partitioner.</exception>
        public virtual IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
        {
            throw new NotSupportedException(Environment.GetResourceString("Partitioner_DynamicPartitionsNotSupported"));
        }

        /// <summary>
        /// Gets whether elements in each partition are yielded in the order of increasing keys.
        /// </summary>
        public bool KeysOrderedInEachPartition { get; private set; }

        /// <summary>
        /// Gets whether elements in an earlier partition always come before elements in a later partition.
        /// </summary>
        /// <remarks>
        /// If <see cref="KeysOrderedAcrossPartitions"/> returns true, each element in partition 0 has a
        /// smaller order key than any element in partition 1, each element in partition 1 has a smaller
        /// order key than any element in partition 2, and so on.
        /// </remarks>
        public bool KeysOrderedAcrossPartitions { get; private set; }

        /// <summary>
        /// Gets whether order keys are normalized.
        /// </summary>
        /// <remarks>
        /// If <see cref="KeysNormalized"/> returns true, all order keys are distinct integers in the range
        /// [0 .. numberOfElements-1]. If the property returns false, order keys must still be dictinct, but
        /// only their relative order is considered, not their absolute values.
        /// </remarks>
        public bool KeysNormalized { get; private set; }

        /// <summary>
        /// Partitions the underlying collection into the given number of ordered partitions.
        /// </summary>
        /// <remarks>
        /// The default implementation provides the same behavior as <see cref="GetOrderablePartitions"/> except
        /// that the returned set of partitions does not provide the keys for the elements.
        /// </remarks>
        /// <param name="partitionCount">The number of partitions to create.</param>
        /// <returns>A list containing <paramref name="partitionCount"/> enumerators.</returns>
        public override IList<IEnumerator<TSource>> GetPartitions(int partitionCount)
        {
            IList<IEnumerator<KeyValuePair<long, TSource>>> orderablePartitions = GetOrderablePartitions(partitionCount);

            if (orderablePartitions.Count != partitionCount)
            {
                throw new InvalidOperationException("OrderablePartitioner_GetPartitions_WrongNumberOfPartitions");
            }

            IEnumerator<TSource>[] partitions = new IEnumerator<TSource>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
            {
                partitions[i] = new EnumeratorDropIndices(orderablePartitions[i]);
            }
            return partitions;
        }

        /// <summary>
        /// Creates an object that can partition the underlying collection into a variable number of
        /// partitions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The returned object implements the <see
        /// cref="T:System.Collections.Generic.IEnumerable{TSource}"/> interface. Calling <see
        /// cref="System.Collections.Generic.IEnumerable{TSource}.GetEnumerator">GetEnumerator</see> on the
        /// object creates another partition over the sequence.
        /// </para>
        /// <para>
        /// The default implementation provides the same behavior as <see cref="GetOrderableDynamicPartitions"/> except
        /// that the returned set of partitions does not provide the keys for the elements.
        /// </para>
        /// <para>
        /// The <see cref="GetDynamicPartitions"/> method is only supported if the <see
        /// cref="System.Collections.Concurrent.Partitioner{TSource}.SupportsDynamicPartitions"/>
        /// property returns true.
        /// </para>
        /// </remarks>
        /// <returns>An object that can create partitions over the underlying data source.</returns>
        /// <exception cref="NotSupportedException">Dynamic partitioning is not supported by this
        /// partitioner.</exception>
        public override IEnumerable<TSource> GetDynamicPartitions()
        {
            IEnumerable<KeyValuePair<long, TSource>> orderablePartitions = GetOrderableDynamicPartitions();
            return new EnumerableDropIndices(orderablePartitions);
        }

        /// <summary>
        /// Converts an enumerable over key-value pairs to an enumerable over values.
        /// </summary>
        private class EnumerableDropIndices : IEnumerable<TSource>, IDisposable
        {
            private readonly IEnumerable<KeyValuePair<long, TSource>> m_source;
            public EnumerableDropIndices(IEnumerable<KeyValuePair<long, TSource>> source)
            {
                m_source = source;
            }
            public IEnumerator<TSource> GetEnumerator()
            {
                return new EnumeratorDropIndices(m_source.GetEnumerator());
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((EnumerableDropIndices)this).GetEnumerator();
            }
            public void Dispose()
            {
                IDisposable d = m_source as IDisposable;
                if (d != null)
                {
                    d.Dispose();
                }
            }
        }

        private class EnumeratorDropIndices : IEnumerator<TSource>
        {
            private readonly IEnumerator<KeyValuePair<long, TSource>> m_source;
            public EnumeratorDropIndices(IEnumerator<KeyValuePair<long, TSource>> source)
            {
                m_source = source;
            }
            public bool MoveNext()
            {
                return m_source.MoveNext();
            }
            public TSource Current
            {
                get
                {
                    return m_source.Current.Value;
                }
            }
            Object IEnumerator.Current
            {
                get
                {
                    return ((EnumeratorDropIndices)this).Current;
                }
            }
            public void Dispose()
            {
                m_source.Dispose();
            }
            public void Reset()
            {
                m_source.Reset();
            }
        }

    }

}
