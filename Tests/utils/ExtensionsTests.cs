using ScriptsOfTribute;

namespace Tests.utils;

public class ExtensionsTests
{
    [Fact]
    void TestSimpleCombinations()
    {
        var list = new List<int> { 1, 2, 3 };
        var expectedResult = new List<List<int>>
        {
            new() { 1, 2 },
            new() { 1, 3 },
            new() { 2, 3 },
        };
        Assert.Equal(expectedResult, list.GetCombinations(2));
    }
    
    [Fact]
    void TestLongList1()
    {
        var list = new List<int> { 1, 2, 3, 4 };
        var expectedResult = new List<List<int>>
        {
            new() { 1, 2, 3 },
            new() { 1, 2, 4 },
            new() { 1, 3, 4 },
            new() { 2, 3, 4 },
        };
        Assert.Equal(expectedResult, list.GetCombinations(3));
    }
    
    [Fact]
    void TestLongList2()
    {
        var list = new List<int> { 1, 2, 3, 4 };
        var expectedResult = new List<List<int>>
        {
            new() { 1, 2 },
            new() { 1, 3 },
            new() { 1, 4 },
            new() { 2, 3 },
            new() { 2, 4 },
            new() { 3, 4 },
        };
        Assert.Equal(expectedResult, list.GetCombinations(2));
    }

    
    [Fact]
    void TestCombinationsOfLength0()
    {
        var list = new List<int> { 1, 2, 3 };
        var expectedResult = new List<List<int>>
        { new() };
        Assert.Equal(expectedResult, list.GetCombinations(0));
    }
    
    [Fact]
    void TestCombinationsOfLength1()
    {
        var list = new List<int> { 1, 2, 3 };
        var expectedResult = new List<List<int>>
        {
            new() { 1 },
            new() { 2 },
            new() { 3 },
        };
        Assert.Equal(expectedResult, list.GetCombinations(1));
    }
    
    [Fact]
    void TestCombinationsOfLengthEqualToArrayLength()
    {
        var list = new List<int> { 1, 2, 3 };
        var expectedResult = new List<List<int>>
        {
            new() { 1, 2, 3 },
        };
        Assert.Equal(expectedResult, list.GetCombinations(3));
    }
    
    [Fact]
    void TestTooLongCombinations()
    {
        var list = new List<int> { 1, 2, 3 };
        var expectedResult = new List<List<int>>
        { };
        Assert.Equal(expectedResult, list.GetCombinations(4));
    }
    
    [Fact]
    void TestSmallArray()
    {
        var list = new List<int> { 1, 2 };
        var expectedResult = new List<List<int>>
        {
            new() { 1, 2 },
        };
        Assert.Equal(expectedResult, list.GetCombinations(2));
    }
    
    [Fact]
    void TestSmallestArray()
    {
        var list = new List<int> { 1 };
        var expectedResult = new List<List<int>>
        {
            new() { 1 },
        };
        Assert.Equal(expectedResult, list.GetCombinations(1));
    }
    
    [Fact]
    void TestSmallestArrayOverflow()
    {
        var list = new List<int> { 1 };
        var expectedResult = new List<List<int>>
        { };
        Assert.Equal(expectedResult, list.GetCombinations(2));
    }
}
