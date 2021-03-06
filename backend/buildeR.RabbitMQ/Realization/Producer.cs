﻿using buildeR.RabbitMq.Interfaces;
using buildeR.RabbitMq.Models;
using RabbitMQ.Client;
using System.Text;

namespace buildeR.RabbitMq.Realization
{
    public class Producer : IProducer
    {
        private readonly IModel _model;
        private readonly PublicationAddress _publicationAddress;

        public Producer(IConnectionFactory connectionFactory, QueueSettings settings)
        {
            _model = new QueueService(connectionFactory, settings).GetModel();
            _publicationAddress = new PublicationAddress(
                    settings.ExchangeType,
                    settings.ExchangeName,
                    settings.RoutingKey);
        }

        public void Send(string message, string type = null)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var properties = _model.CreateBasicProperties();
            properties.Persistent = true;
            if (!string.IsNullOrEmpty(type))
            {
                properties.Type = type;
            }

            _model.BasicPublish(_publicationAddress, properties, bytes);
        }
    }
}
