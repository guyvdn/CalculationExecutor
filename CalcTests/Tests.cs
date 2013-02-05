using Calc;
using NUnit.Framework;

namespace CalcTests
{
[TestFixture]
public class Tests
{
    [Test]
    public void Should_execute_all_methods_in_correct_order()
    {
        var entity = new EntityClass{ PropertyA = 1, PropertyC= 5};
        var calculator = new EntityClassCalculator();

        var executor = new CalculationExecutor<EntityClass, EntityClassCalculator>(calculator);
        executor.Execute(entity);

        Assert.That(entity.PropertyA, Is.EqualTo(1));
        Assert.That(entity.PropertyB, Is.EqualTo(3));
        Assert.That(entity.PropertyC, Is.EqualTo(5));
        Assert.That(entity.PropertyD, Is.EqualTo(13));
        Assert.That(entity.PropertyE, Is.EqualTo(26));
        Assert.That(entity.PropertyF, Is.EqualTo(139));
        Assert.That(entity.PropertyG, Is.EqualTo(50));
    }
}
}
