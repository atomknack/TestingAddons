using System;
using System.Collections;
using System.Collections.Generic;
using CollectionLike;
using FluentAssertions;

namespace FluentAssertions_Extensions
{
    public static partial class Assertions
    {

        public static void ShouldContainEqualElements<T>(this ReadOnlySpan<T> actual, ReadOnlySpan<T> expected)
        {
            actual.Length.Should().Be(expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                var item = expected[i];
                actual.Count(x => x.Equals(item)).Should().Be(expected.Count(x => x.Equals(item)));
            }
        }

        public static void ShouldEqual<T>(this Span<T> actual, Span<T> expected) =>
            ((ReadOnlySpan<T>)actual).ShouldEqual((ReadOnlySpan<T>)expected);
        //public static void ShouldEqual<T>(this Span<T> actual, ReadOnlySpan<T> expected) =>
        //    ((ReadOnlySpan<T>)actual).ShouldEqual(expected);
        public static void ShouldEqual<T>(this ReadOnlySpan<T> actual, ReadOnlySpan<T> expected)
        {
            //actual.ToArray().Should().Equal(expected.ToArray());

            actual.Length.Should().Be(expected.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                actual[i].Should().Be(expected[i], $"it located at [{i}]index");
            }

        }

        /*
        public static void AreEqual<T>(Span<T> expected, Span<T> actual) =>
        AreEqual((ReadOnlySpan<T>)expected, (ReadOnlySpan<T>)actual);
        public static void AreEqual<T>(ReadOnlySpan<T> expected, ReadOnlySpan<T> actual)
        {
            AreEqual(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                AreEqual(expected[i], actual[i]);
            }
            //Debug.Log($"AssertAreEqual seccessfully for {expected.Length} items");
        }
        */
    }

}