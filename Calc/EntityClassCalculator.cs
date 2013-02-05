using System.Diagnostics;

namespace Calc
{
public interface ICalculateEntity<TEntity> { };

public class EntityClassCalculator
    : ICalculateEntity<EntityClass>
{
    [DependsOn("PropertyD", "PropertyE")]
    public void CalculatePropertyF(EntityClass entity)
    {
        Debug.WriteLine("F calculated");
        entity.PropertyF = entity.PropertyD + entity.PropertyE +
                            100;
    }

    [DependsOn("PropertyB", "PropertyD")]
    public void CalculatePropertyE(EntityClass entity)
    {
        Debug.WriteLine("E calculated");
        entity.PropertyE = entity.PropertyB + entity.PropertyD + 10;
    }

    public void CalculatePropertyG(EntityClass entity)
    {
        Debug.WriteLine("G calculated");
        entity.PropertyG = 50;
    }

    [DependsOn("PropertyB", "PropertyC")]
    public void CalculatePropertyD(EntityClass entity)
    {
        Debug.WriteLine("D calculated");
        entity.PropertyD = entity.PropertyB + entity.PropertyC + 5;
    }

    [DependsOn("PropertyA")]
    public void CalculatePropertyB(EntityClass entity)
    {
        Debug.WriteLine("B calculated");
        entity.PropertyB = entity.PropertyA + 2;
    }

    // Enable this to test CircularDependency
    //[DependsOn("PropertyE")]
    //public void CalculatePropertyA(EntityClass entity)
    //{
    //    Debug.WriteLine("A calculated");
    //    entity.PropertyA = 1;
    //}
}
}