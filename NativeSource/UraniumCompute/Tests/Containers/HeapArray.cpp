#include <Tests/Common/Common.h>
#include <UnCompute/Containers/HeapArray.h>

using namespace UN;

TEST(HeapArray, EmptyConstructor)
{
    HeapArray<Int32> array;
    EXPECT_EQ(array.Length(), 0);
    EXPECT_TRUE(array.Empty());
    EXPECT_FALSE(array.Any());
}

TEST(HeapArray, CreateFromArraySlice)
{
    std::vector<Int32> vector = { 1, 2, 3, 4, 5, 1 };
    ArraySlice<Int32> slice   = vector;
    HeapArray<Int32> array    = slice.AsConst();

    EXPECT_EQ(array.Length(), vector.size());

    for (USize i = 0; i < array.Length(); ++i)
    {
        EXPECT_EQ(array[i], vector[i]);
    }

    array[0] = 123;
    EXPECT_EQ(array[0], 123);

    auto sub = array(0, 3);
    EXPECT_EQ(sub[0], array[0]);
    EXPECT_EQ(sub[1], array[1]);
    EXPECT_EQ(sub[2], array[2]);

    sub[0] = 1;
    EXPECT_EQ(sub[0], 1);
    EXPECT_EQ(array[0], 1);

    EXPECT_EQ(array.IndexOf(2), 1);
    EXPECT_TRUE(array.Contains(2));

    EXPECT_EQ(array.IndexOf(25), -1);
    EXPECT_FALSE(array.Contains(25));

    EXPECT_EQ(array.IndexOf(1), 0);
    EXPECT_EQ(array.LastIndexOf(1), 5);
}

TEST(HeapArray, CreateFromVector)
{
    std::vector<Int32> vector = { 1, 2, 3, 4, 5, 1 };
    HeapArray<Int32> array    = ArraySlice<const Int32>(vector);

    EXPECT_EQ(array.Length(), vector.size());

    for (USize i = 0; i < array.Length(); ++i)
    {
        EXPECT_EQ(array[i], vector[i]);
    }
}
