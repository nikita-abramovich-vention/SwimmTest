using System.Threading;
using System.Threading.Tasks;
using DreamTeam.Common;
using DreamTeam.Microservices.Communication.RabbitMq.Messaging;
using DreamTeam.Microservices.Interfaces;
using DreamTeam.Wod.EmployeeService.ConfigurationOptions;
using DreamTeam.Wod.EmployeeService.Foundation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DreamTeam.Wod.EmployeeService
{
    [UsedImplicitly]
    public sealed class EmployeeHostedService : IHostedService
    {
        private readonly IEmployeeMicroservice _employeeMicroservice;
        private readonly EmployeeServiceOptions _employeeServiceOptions;
        private readonly ICommunicationService _communicationService;
        private readonly IRabbitMqMessageTransportFactory _messageTransportFactory;


        public EmployeeHostedService(
            IEmployeeMicroservice employeeMicroservice,
            IOptionsMonitor<EmployeeServiceOptions> employeeServiceOptionsAccessor,
            ICommunicationService communicationService,
            IRabbitMqMessageTransportFactory messageTransportFactory)
        {
            _employeeMicroservice = employeeMicroservice;
            _employeeServiceOptions = employeeServiceOptionsAccessor.CurrentValue;
            _communicationService = communicationService;
            _messageTransportFactory = messageTransportFactory;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var messageTransport = await _messageTransportFactory.CreateMessageTransportAsync(_employeeServiceOptions.MqQueueName);
            await _communicationService.StartAsync(_employeeMicroservice, messageTransport);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _communicationService.StopAsync();
        }
    }
}