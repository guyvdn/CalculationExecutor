using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Calc
{
public interface ICalculationExecutor<in TEntity, out TCalculator>
    where TEntity : class
    where TCalculator : ICalculateEntity<TEntity>
{
    void Execute(TEntity entity);
    TCalculator Calculator { get; }
};


public class CalculationExecutor<TEntity, TCalculator>
    : ICalculationExecutor<TEntity, TCalculator>
    where TEntity : class
    where TCalculator : ICalculateEntity<TEntity>
{
    private readonly TCalculator calculator;
    private List<MethodInfo> allMethodsToExecute;

    public CalculationExecutor(TCalculator calculator)
    {
        this.calculator = calculator;
    }

    public TCalculator Calculator
    {
        get { return calculator; }
    }

    public void Execute(TEntity entity)
    {
        allMethodsToExecute = GetMethodsToExecute(calculator).ToList();

        while (allMethodsToExecute.Any())
        {
            var methodToExecute = FindNextMethodToExecute(allMethodsToExecute);
            methodToExecute.Invoke(calculator, new object[] {entity});
            allMethodsToExecute.Remove(methodToExecute);
        }
    }

    private static IEnumerable<MethodInfo> GetMethodsToExecute(
        ICalculateEntity<TEntity> calculator)
    {
        return calculator
            .GetType()
            .GetMethods()
            .Where(m => m.Name.StartsWith("Calculate"));
    }

    private MethodInfo FindNextMethodToExecute(
        IEnumerable<MethodInfo> methodsToExecute)
    {
        var nextMethod = methodsToExecute.First();

        var dependsOn =
            (DependsOnAttribute)
            nextMethod.GetCustomAttributes(typeof (DependsOnAttribute), false)
                      .SingleOrDefault();

        if (dependsOn == null)
            return nextMethod;

        var dependentMethods = new List<MethodInfo>();
        var dependencies = dependsOn.Dependencies;
        foreach (var dependency in dependencies)
        {
            var dependentMethodName = "Calculate" + dependency;
            if (allMethodsToExecute.Any(m => m.Name == dependentMethodName))
            {
                dependentMethods.Add(
                    allMethodsToExecute.First(
                        m => m.Name == dependentMethodName));
            }
        }

        if (!dependentMethods.Any())
            return nextMethod;

        return FindNextMethodToExecute(dependentMethods);
    }
}
}