#include <Tests/Common/Common.h>
#include <UnCompute/Containers/ArraySlice.h>
#include <UnCompute/Containers/HeapArray.h>

using namespace UN;

TEST(ArraySlice, EmptyConstructor)
{
    ArraySlice<Int32> slice;
    EXPECT_EQ(slice.Length(), 0);
    EXPECT_TRUE(slice.Empty());
    EXPECT_FALSE(slice.Any());
}

TEST(ArraySlice, CreateFromVector)
{
    std::vector<Int32> vector = { 1, 2, 3, 4, 5, 1 };
    ArraySlice<Int32> slice   = vector;

    EXPECT_EQ(slice.Length(), vector.size());

    for (USize i = 0; i < slice.Length(); ++i)
    {
        EXPECT_EQ(slice[i], vector[i]);
    }

    slice[0] = 123;
    EXPECT_EQ(slice[0], 123);
    EXPECT_EQ(vector[0], 123);

    auto sub = slice(0, 3);
    EXPECT_EQ(sub[0], slice[0]);
    EXPECT_EQ(sub[1], slice[1]);
    EXPECT_EQ(sub[2], slice[2]);

    sub[0] = 1;
    EXPECT_EQ(sub[0], 1);
    EXPECT_EQ(slice[0], 1);
    EXPECT_EQ(vector[0], 1);

    EXPECT_EQ(slice.IndexOf(2), 1);
    EXPECT_TRUE(slice.Contains(2));

    EXPECT_EQ(slice.IndexOf(25), -1);
    EXPECT_FALSE(slice.Contains(25));

    EXPECT_EQ(slice.IndexOf(1), 0);
    EXPECT_EQ(slice.LastIndexOf(1), 5);
}

TEST(ArraySlice, CreateFromConstVector)
{
    std::vector<Int32> vector     = { 1, 2, 3, 4, 5, 1 };
    ArraySlice<const Int32> slice = vector;

    EXPECT_EQ(slice.Length(), vector.size());

    for (USize i = 0; i < slice.Length(); ++i)
    {
        EXPECT_EQ(slice[i], vector[i]);
    }

    // won't work (and shouldn't)
    // slice[0] = 123;

    auto sub = slice(0, 3);
    EXPECT_EQ(sub[0], slice[0]);
    EXPECT_EQ(sub[1], slice[1]);
    EXPECT_EQ(sub[2], slice[2]);

    // same
    // sub[0] = 1;

    EXPECT_EQ(slice.IndexOf(2), 1);
    EXPECT_TRUE(slice.Contains(2));

    EXPECT_EQ(slice.IndexOf(25), -1);
    EXPECT_FALSE(slice.Contains(25));

    EXPECT_EQ(slice.IndexOf(1), 0);
    EXPECT_EQ(slice.LastIndexOf(1), 5);

    std::vector<Int32> copy;
    copy.resize(slice.Length(), 0);
    ArraySlice<Int32> mutableSlice = copy;
    slice.CopyDataTo(mutableSlice);

    for (USize i = 0; i < slice.Length(); ++i)
    {
        EXPECT_EQ(copy[i], vector[i]);
    }
}

inline SSize TestIndexOf(ArraySlice<const Int32> source, Int32 value)
{
    return source.IndexOf(value);
}

inline void TestSetFirst(ArraySlice<Int32> source, Int32 value)
{
    source[0] = value;
}

TEST(ArraySlice, FunctionParameter)
{
    std::vector<Int32> vector = { 1, 2, 3 };
    EXPECT_EQ(TestIndexOf(vector, 2), 1);
    auto heapArray = HeapArray<Int32>::CopyFrom({ 1, 2, 3 });
    EXPECT_EQ(TestIndexOf(heapArray, 2), 1);
    EXPECT_EQ(TestIndexOf({ 1, 2, 3 }, 2), 1);

    TestSetFirst(vector, 123);
    EXPECT_EQ(vector[0], 123);
    TestSetFirst(heapArray, 123);
    EXPECT_EQ(heapArray[0], 123);

    // won't work (and shouldn't)
    // TestSetFirst({ 1, 2, 3 }, 123);
}
