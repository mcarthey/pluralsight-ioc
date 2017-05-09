using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IOC.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Problem is we have to resolve the type prior to use
            //ICreditCard creditCard = new MasterCard();
            //ICreditCard otherCreditCard = new Visa();
            //var shopper = new Shopper(otherCreditCard);

            // Lets try another way
            // What if we have another type figure out what we need?
            // (eg. business rules, config settings, etc)
            // issue is we don't want a method to resolve every type
            //Resolver resolver = new Resolver();
            //var shopper = new Shopper(resolver.ResolveCreditCard());

            // let the resolver figure out the type to create
            Resolver resolver = new Resolver();
            resolver.Register<Shopper, Shopper>();
            resolver.Register<ICreditCard, Visa>();
            var shopper = resolver.Resolve<Shopper>();

            shopper.Charge();

        }
    }

    public class Resolver
    {
        // map from one type to another
        private Dictionary<Type, Type> dependencyMap = new Dictionary<Type, Type>();

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        private object Resolve(Type typeToResolve)
        {
            Type resolvedType = null;
            try
            {
                resolvedType = dependencyMap[typeToResolve];
            }
            catch
            {
                throw new Exception(string.Format("Could not resolve type {0}", typeToResolve.FullName));
            }

            var firstConstructor = resolvedType.GetConstructors().First();
            var constructorParameters = firstConstructor.GetParameters();

            // if not parameters just instantiate using default constructor
            if (constructorParameters.Length == 0)
            {
                return Activator.CreateInstance(resolvedType);
            }

            IList<object> parameters = new List<object>();
            foreach (var parameterToResolve in constructorParameters)
            {
                parameters.Add(Resolve(parameterToResolve.ParameterType));
            }

            return firstConstructor.Invoke(parameters.ToArray());
        }

        //public ICreditCard ResolveCreditCard()
        //{
        //    if (new Random().Next(2) == 1)
        //        return new Visa();
        //    return new MasterCard();
        //}

        public void Register<TFrom, TTo>()
        {
            dependencyMap.Add(typeof(TFrom), typeof(TTo));
        }
    }
    public class Visa : ICreditCard
    {
        public string Charge()
        {
            return "Charging with the Visa!";
        }
    }

    public class MasterCard : ICreditCard
    {
        public string Charge()
        {
            return "Swiping the MasterCard!";
        }
    }

    public class Shopper
    {
        private readonly ICreditCard _creditCard;

        public Shopper(ICreditCard creditCard)
        {
            _creditCard = creditCard;
        }

        public void Charge()
        {
            var chargeMessage = _creditCard.Charge();
            Console.WriteLine(chargeMessage);
        }
    }

    public interface ICreditCard
    {
        string Charge();
    }
}
