using Castle.Windsor;
using SharpArch.Core.CommonValidator;
using SharpArch.Data.NHibernate;
using SharpArch.Core.PersistenceSupport;
using SharpArch.Core.NHibernateValidator.CommonValidatorAdapter;
using CommonServiceLocator.WindsorAdapter;
using Microsoft.Practices.ServiceLocation;
using HatCMS.Core.DataInterface;
using HatCMS.Core.DataRepository;


namespace HatCMS
{
    using Castle.MicroKernel.Registration;

    public class ServiceLocatorInitializer
    {
        public static void Init()
        {
            IWindsorContainer container = new WindsorContainer();

            container.Register(
                    Component
                        .For(typeof(IValidator))
                        .ImplementedBy(typeof(Validator))
                        .Named("validator"));

            container.Register(
                    Component
                        .For(typeof(ISessionFactoryKeyProvider))
                        .ImplementedBy(typeof(DefaultSessionFactoryKeyProvider))
                        .Named("sessionFactoryKeyProvider"));

            container.Register(
                    Component
                        .For(typeof(IEntityDuplicateChecker))
                        .ImplementedBy(typeof(EntityDuplicateChecker))
                        .Named("entityDuplicateChecker"));

            container.Register(
                Component
                    .For(typeof(IPersistenceVariableRepository))
                    .ImplementedBy(typeof(PersistentVariableRepository))
                    .Named("persistencevariableRepository"));

            container.Register(
                Component
                    .For(typeof(IPageSecurityZoneUserRoleRepository))
                    .ImplementedBy(typeof(PageSecurityZoneUserRoleRepository))
                    .Named("pagesecurityzoneuserroleRepository"));

            container.Register(
                Component
                    .For(typeof(IPageRepository))
                    .ImplementedBy(typeof(PageRepository))
                    .Named("pageRepository"));





            ServiceLocator.SetLocatorProvider(delegate
            {
                return new WindsorServiceLocator(container);
            });
        }
    }
}
