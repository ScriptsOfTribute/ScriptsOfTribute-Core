using TalesOfTribute;

namespace Tests.Board;

public class UniqueIdTests
{
    [Fact]
    void ShouldCreateUniqueIdsWithConsecutiveValues()
    {
        var id1 = UniqueId.Create();
        var id2 = UniqueId.Create();
        Assert.Equal(100, id1.Value);
        Assert.Equal(101, (int)id2);
    }
    
    [Fact]
    void EqualityOperatorsShouldWorkCorrectly()
    {
        var id1 = UniqueId.Create();
        var id1copy = id1;
        var id2 = UniqueId.Create();
        Assert.True(id1 == id1copy);
        Assert.False(id1 != id1copy);
        Assert.True(id1 != id2);
        Assert.False(id1 == id2);
        Assert.True(id1 == id1);
    }

    [Fact]
    void EmptyShouldEqualItselfButNothingElse()
    {
        var empty1 = UniqueId.Empty;
        var empty2 = UniqueId.Empty;
        var id1 = UniqueId.Create();
        var id2 = UniqueId.Create();
        
        Assert.True(empty1 == empty2);
        Assert.False(empty1 != empty2);
        Assert.False(empty1 == id1);
        Assert.False(empty1 == id2);
        Assert.True(empty1 != id1);
        Assert.True(empty1 != id2);
    }
}
