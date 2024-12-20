using Application;
using Autofac;
using Domain.Common.Base;
using Domain.Common.Contracts;
using FluentValidation;

namespace API.Extentions;
public static class AutofacConfigurationExtensions
{
    public static void AddServices(this ContainerBuilder containerBuilder)
    {

        containerBuilder.RegisterGeneric(typeof(EnumService<,>)).As(typeof(IEnumService<,>)).InstancePerDependency();

        var entityAssembly = typeof(IEntity).Assembly;
        //var dataAssembly = typeof(TRSContext).Assembly;
        var applicationPointer = typeof(ApplicationPointer).Assembly;
        //var dataReadOnlyAssembly = typeof(TRSReadOnlyContext).Assembly;


        containerBuilder.RegisterAssemblyTypes(applicationPointer)
            .Where(type => type.BaseType != null &&
                           type.BaseType.IsGenericType &&
                           type.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            .AsImplementedInterfaces()
            .InstancePerDependency();



        containerBuilder.RegisterAssemblyTypes(entityAssembly, applicationPointer)
            .AssignableTo<IScopedDependency>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        containerBuilder.RegisterAssemblyTypes(entityAssembly, applicationPointer)
            .AssignableTo<ITransientDependency>()
            .AsImplementedInterfaces()
            .InstancePerDependency();

        containerBuilder.RegisterAssemblyTypes(entityAssembly, applicationPointer)
            .AssignableTo<ISingletonDependency>()
            .AsImplementedInterfaces()
            .SingleInstance();


    }
}