﻿using System.Linq;
using Denifia.Stardew.SendItems.Services;
using Autofac;
using Denifia.Stardew.SendItems.Domain;
using StardewModdingAPI;
using System;
using StardewModdingAPI.Events;

namespace Denifia.Stardew.SendItems
{
    public class SendItems : Mod
    {
        private IContainer _container;
        private IFarmerService _farmerService;
        private IPostboxInteractionDetector _postboxInteractionDetector;
        private ILetterboxInteractionDetector _letterboxInteractionDetector;
        private IMailDeliveryService _mailDeliveryService;
        private IMailCleanupService _mailCleanupService;

        public override void Entry(IModHelper helper)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(this).As<IMod>();
            builder.RegisterInstance(helper).As<IModHelper>();
            builder.Register(c => new VersionCheckService(c.Resolve<IMod>()));
            builder.RegisterAssemblyTypes(typeof(SendItems).Assembly)
                .Where(t => t.Name.EndsWith("Service")) 
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(SendItems).Assembly)
                .Where(t => t.Name.EndsWith("Detector"))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            _container = builder.Build();

            // Init repo first!
            Repository.Instance.Init(_container.Resolve<IConfigurationService>());

            // Instance classes that do their own thing
            _container.Resolve<IFarmerService>();
            _container.Resolve<VersionCheckService>();
            _container.Resolve<ICommandService>();
            _container.Resolve<IPostboxService>();
            _container.Resolve<ILetterboxService>();
            _container.Resolve<IPostboxInteractionDetector>();
            _container.Resolve<ILetterboxInteractionDetector>();
            _container.Resolve<IMailDeliveryService>();
            _container.Resolve<IMailCleanupService>();
            _container.Resolve<IMailScheduleService>();
        }
    }
}
