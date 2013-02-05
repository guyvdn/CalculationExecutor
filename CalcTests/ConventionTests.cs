using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Calc;
using NUnit.Framework;

namespace CalcTests
{
    public static class ExtensionMethods
    {
        public static bool Implements(this Type[] types, Type typeToImplement)
        {
            return types.Any(t => t.Implements(typeToImplement));
        }

        public static bool Implements(this Type i, Type typeToImplement)
        {
            return i.IsGenericType && i.GetGenericTypeDefinition() == typeToImplement;
        }
    }

    public struct Tuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    [TestFixture]
    public class EntityCalculatorTests
    {
        [TestFixture]
        public class ConventionTests
        {
            private IEnumerable<Type> calculators;

            [TestFixtureSetUp]
            public void GetCalculators()
            {
                var assembly = typeof(EntityClassCalculator).Assembly;

                calculators = assembly
                    .GetTypes()
                    .Where(type => type.GetInterfaces().Implements(typeof (ICalculateEntity<>)));
            }

            [Test]
            public void Methods_of_the_calculators_should_not_have_dependencies_on_theirself()
            {
                foreach (var calculator in calculators)
                {
                    var dependencyDictionary = new List<Tuple<string, string>>();
                    var methods = GetMethodsOfCalculator(calculator);
                    foreach (var method in methods)
                    {
                        var dependsOn = GetDependsOnAttributeOfMethod(method);
                        if (dependsOn == null)
                            continue;

                        var dependencies = dependsOn.Dependencies;
                        dependencyDictionary.AddRange(
                            dependencies.Select(
                                dependency => new Tuple<string, string>(method.Name, "Calculate" + dependency)));
                    }

                    AssertDependencyDictionary(dependencyDictionary, calculator.Name);
                }
            }

            private static void AssertDependencyDictionary(List<Tuple<string, string>> dependencyDictionary,
                                                           string calculatorName)
            {
                var methodsWithDirectDependencyOnItsSelf = dependencyDictionary.Where(t => t.Item1 == t.Item2)
                                                                               .Select(t => t.Item1)
                                                                               .ToList();

                Assert.That(methodsWithDirectDependencyOnItsSelf.Count(), Is.EqualTo(0),
                            "Method(s) " + String.Join(", ", methodsWithDirectDependencyOnItsSelf) +
                            " of calculator " +
                            calculatorName + " has dependency on itself");


                foreach (var dependency in dependencyDictionary)
                {
                    AssertCircularDependency(new List<string>(), dependency, dependencyDictionary, calculatorName);
                }

            }

            private static void AssertCircularDependency(IEnumerable<string> methods,
                                                         Tuple<string, string> dependency,
                                                         List<Tuple<string, string>> dependencies,
                                                         string calculatorName)
            {
                var method = dependency.Item1;
                var methodsList = methods.ToList();

                if (methodsList.Any(m => m == dependency.Item1))
                {
                    var methodsString = string.Join(", ", methodsList);
                    var circularString =
                        methodsString.Substring(methodsString.IndexOf(method, StringComparison.Ordinal)) + ", " +
                        method;

                    Assert.Fail("Method " + dependency.Item1 + " of calculator " + calculatorName +
                                " has a circular reference to itself via " + circularString);
                }

                methodsList.Add(method);

                var childDependencies = dependencies.Where(d => d.Item1 == dependency.Item2).ToList();
                foreach (var child in childDependencies)
                {
                    AssertCircularDependency(methodsList, child, dependencies, calculatorName);
                }
            }


            [Test]
            public void All_methods_and_dependencies_of_the_calculators_should_be_named_properly()
            {
                foreach (var calculator in calculators)
                {
                    var entityToCalculate = GetEntityToCalculate(calculator);
                    var entityProperties = GetPropertiesOfEntity(entityToCalculate);

                    var methods = GetMethodsOfCalculator(calculator);
                    foreach (var method in methods)
                    {
                        Assert.That(method.Name.StartsWith("Calculate"),
                                    "Method " + method.Name + " for calculator " + calculator.Name +
                                    " should start with Calculate");

                        Assert.That(entityProperties.Any(p => p.Name == method.Name.Substring(9)),
                                    "Property " + method.Name.Substring(9) +
                                    " of method " + method.Name +
                                    " for calculator " + calculator.Name +
                                    " was not found in " + entityToCalculate.Name);

                        var dependsOn = GetDependsOnAttributeOfMethod(method);
                        if (dependsOn == null)
                            continue;

                        var dependencies = dependsOn.Dependencies;
                        foreach (var dependency in dependencies)
                        {
                            Assert.That(entityProperties.Any(p => p.Name == dependency),
                                        "Dependency " + dependency +
                                        " of method " + method.Name +
                                        " for calculator " + calculator.Name +
                                        " was not found in " + entityToCalculate.Name);
                        }
                    }
                }
            }

            private static DependsOnAttribute GetDependsOnAttributeOfMethod(MethodInfo method)
            {
                return
                    (DependsOnAttribute)
                    method.GetCustomAttributes(typeof (DependsOnAttribute), false).SingleOrDefault();
            }

            private static PropertyInfo[] GetPropertiesOfEntity(Type entityToCalculate)
            {
                var entityProperties = entityToCalculate.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                return entityProperties;
            }

            private static Type GetEntityToCalculate(Type calculator)
            {
                var iCalculateEntityInterface = calculator
                    .GetInterfaces()
                    .Single(t => t.Implements(typeof (ICalculateEntity<>)));

                var entityToCalculate = iCalculateEntityInterface.GetGenericArguments().First();
                return entityToCalculate;
            }

            private static IEnumerable<MethodInfo> GetMethodsOfCalculator(Type calculator)
            {
                return calculator.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            }
        }
    }
}