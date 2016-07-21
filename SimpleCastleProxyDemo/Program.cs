using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using FluentAssertions;

namespace SimpleCastleProxyDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // create out user instance
            var userInstance = new User("role1", "role2");

            // create another variable with the user as it's interface
            var user = userInstance as IUser;

            // assertions
            user.IsInRole("role1").Should().BeTrue();
            user.IsInRole("role2").Should().BeTrue();
            user.IsInRole("role3").Should().BeFalse();

            // create interceptor to add roles
            var addRole3Interceptor = new AlsoInRolesInterceptor("role3");

            // generate the Castle proxy, passing in our interceptor, and assigning back to the interface version of the user
            var generator = new Castle.DynamicProxy.ProxyGenerator();
            user = generator.CreateInterfaceProxyWithTarget<IUser>(userInstance, addRole3Interceptor);

            // assertions
            user.IsInRole("role1").Should().BeTrue();
            user.IsInRole("role2").Should().BeTrue();
            user.IsInRole("role3").Should().BeTrue(); // tada!

            Console.WriteLine("Success");
            Console.ReadLine();
        }
    }

    public interface IUser
    {
        bool IsInRole(string role);
    }

    public class User : IUser
    {
        private string[] roles;

        public User(params string[] roles)
        {
            this.roles = roles;
        }

        public bool IsInRole(string role)
        {
            return roles.Contains(role);
        }
    }

    internal class AlsoInRolesInterceptor : IInterceptor
    {
        private IEnumerable<string> roles;

        public AlsoInRolesInterceptor(params string[] roles)
        {
            this.roles = roles;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            if (!(bool)invocation.ReturnValue)
            {
                invocation.ReturnValue = this.roles.Contains(invocation.Arguments[0]);
            }
        }
    }
}