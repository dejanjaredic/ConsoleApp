using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace FirstConsole
{
    class Program
    {
        private static Entitet[] lista = new Entitet[]
            {new Entitet() {Id = 1, Name = "Jedan"}, new Entitet() {Id = 2, Name = "Dva"}};

        static void Main(string[] args)
        {
            //Jedan Komentar
            //Drugi Komentar

            var operators = new List<string>
                {
                    "gt",
                    "lt",
                    "gte",
                    "lte",
                    "eq",
                    "ct",
                    "sw",
                    "ew"
                };

            Console.WriteLine("Unesi vrijednost parametra:");
            var stringValue = Console.ReadLine();

            Console.WriteLine("Izaberi jedan od sledecih operatora:");
            Console.WriteLine(string.Join(", ", operators));
            var op = Console.ReadLine();

            Console.WriteLine("Unesi naziv propertija nad kojim vrsis upit");
            var propName = Console.ReadLine();

            var expression = GetWhereExpression<Entitet>(op, propName, stringValue);
            var result = lista.AsQueryable().Where(expression);

            Console.WriteLine(string.Join(", ", result));
            Console.ReadLine();
        }

        ////static void Main(string[] args)
        ////{
        ////    // x => x.Nesto > 5

        ////    var operators = new List<string>
        ////    {
        ////        "gt",
        ////        "lt",
        ////        "gte",
        ////        "lte"
        ////    };

        ////    var niz = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        ////    Console.WriteLine("Unesi broj n:");
        ////    var n = int.Parse(Console.ReadLine());

        ////    Console.WriteLine("Izaberi jedan od sledecih operatora:");
        ////    Console.WriteLine(string.Join(", ", operators));

        ////    var op = Console.ReadLine();
        ////    var lambda = GetWhereExpression(op, n);

        ////    var dynrezultat = niz.AsQueryable().Where(lambda);

        ////    Console.WriteLine(string.Join(", ", dynrezultat));
        ////    Console.ReadLine();
        ////}

        private static Expression<Func<TEntity, bool>> GetWhereExpression<TEntity>(string op, string propertyName, string value)
        {
            // x =>
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "x");

            // x.propertyName <op> <value>
            // x.Id > value
            // x.Name == value

            // x.PropertyName
            var propExpression = Expression.Property(parameter, propertyName);
            // Vraca tip property
            var type = propExpression.Type;
            //Konvertuje string u tip promjenljive
            var convertedValue = Convert.ChangeType(value, type);
            //Sadrzi vrijednost tipa promjenljive
            var constant = Expression.Constant(convertedValue);
            
            BinaryExpression binary;

            switch (convertedValue)
            {
                case string _:
                    binary =  GetBinaryExpressionForString(op, propExpression, constant);
                    break;
                case int _:
                    binary = GetBinaryExpressionForInt(op, propExpression, constant);
                    break;
                default:
                    throw new ArgumentException($"Neocekivani tip vrijednosti '{type.Name}'");
            }

            return Expression.Lambda<Func<TEntity, bool>>(binary, parameter);
        }

        private static BinaryExpression GetBinaryExpressionForInt(string op, MemberExpression propExpression, ConstantExpression constant)
        {
            switch (op)
            {
                case "gt":
                    return Expression.GreaterThan(propExpression, constant);
                case "lt":
                    return Expression.LessThan(propExpression, constant);
                case "gte":
                    return Expression.GreaterThanOrEqual(propExpression, constant);
                case "lte":
                    return Expression.LessThanOrEqual(propExpression, constant);
                case "eq":
                    return Expression.Equal(propExpression, constant);
                
                default:
                    throw new InvalidOperationException($"Neocekivani operator {op}");
            }
        }

        //private static BinaryExpression GetBinaryExpressionForString(string op, MemberExpression propExpression, ConstantExpression constant)
        //{
        //    BinaryExpression bin;

        //    switch (op)
        //    {
        //        case "eq":
        //            MethodInfo methodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        //            var contains = Expression.Call(propExpression, methodInfo, constant);
        //        default:
        //            throw new InvalidOperationException($"Neocekivani operator {op}");
        //    }
        //}

        private static BinaryExpression GetBinaryExpressionForString(string op, MemberExpression propExpression,
            ConstantExpression constant)
        {
            var trueExpression = Expression.Constant(true, typeof(bool));
            BinaryExpression bin;

            switch (op)
            {
                case "eq":
                    return Expression.Equal(propExpression, constant);
                case "ct":
                    MethodInfo methodInfo1 = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var contains = Expression.Call(propExpression, methodInfo1, constant);
                    bin = Expression.Equal(contains, trueExpression);
                    break;

                case "sw":
                    MethodInfo methodInfo2 = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                    var startsWith = Expression.Call(propExpression, methodInfo2, constant);
                    bin = Expression.Equal(startsWith, trueExpression);
                    break;

                case "ew":
                    MethodInfo methodInfo3 = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                    var endsWith = Expression.Call(propExpression, methodInfo3, constant);
                    bin = Expression.Equal(endsWith, trueExpression);
                    break;

                default:
                    throw new InvalidOperationException($"Neocekivani operator {op}");
            }

            return bin;
        }
        

        //private static Expression<Func<int, bool>> GetWhereExpression(string op, int constant)
        //{
        //    // num =>
        //    ParameterExpression numParam = Expression.Parameter(typeof(int), "num");

        //    // n
        //    ConstantExpression constPar = Expression.Constant(constant, typeof(int));

        //    BinaryExpression binExp;
        //    switch (op)
        //    {
        //        case "gt":
        //            binExp = Expression.GreaterThan(numParam, constPar);
        //            break;
        //        case "lt":
        //            binExp = Expression.LessThan(numParam, constPar);
        //            break;
        //        case "gte":
        //            binExp = Expression.GreaterThanOrEqual(numParam, constPar);
        //            break;
        //        case "lte":
        //            binExp = Expression.LessThanOrEqual(numParam, constPar);
        //            break;
        //        default:
        //            throw new InvalidOperationException($"Neocekivani operator {op}");
        //    }

        //    return Expression.Lambda<Func<int, bool>>(binExp, numParam);
        //}
    }
}
