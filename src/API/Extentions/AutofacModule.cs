using Autofac;
using FluentValidation;
using System.Reflection;

namespace API.Extentions;

public class AutofacModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var validatorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.BaseType != null &&
                           type.BaseType.IsGenericType &&
                           type.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>));

        foreach (var validatorType in validatorTypes)
        {
            var genericArgument = validatorType.BaseType.GetGenericArguments().FirstOrDefault();

            if (genericArgument is not null)
            {
                builder.RegisterType(validatorType)
                    .As(typeof(IValidator<>).MakeGenericType(genericArgument))
                    .InstancePerDependency();
            }
        }
    }
}