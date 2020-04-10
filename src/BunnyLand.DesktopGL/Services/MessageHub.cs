using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;

namespace BunnyLand.DesktopGL.Services
{
    // Inspired by https://github.com/upta/pubsub/blob/master/PubSub/Core/Hub.cs and MediatR
    public class MessageHub
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> notificationHandlers = new ConcurrentDictionary<Type, List<Delegate>>();
        private readonly ConcurrentDictionary<Type, Delegate> requestHandlers = new ConcurrentDictionary<Type, Delegate>();

        private IDictionary<Type, List<Delegate>> NotificationHandlers => notificationHandlers;
        private IDictionary<Type, Delegate> RequestHandlers => requestHandlers;

        public void Publish<T>(T notification) where T : INotification => NotificationHandlers
            .TryGetValue(typeof(T)).IfSome(handlers => handlers.ForEach(handler => {
                switch (handler) {
                    case Action<T> action:
                        action(notification);
                        break;
                    case Func<T, Task> func:
                        func(notification).GetAwaiter().GetResult();
                        break;
                }
            }));

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request) => await RequestHandlers
            .TryGetValue(request.GetType())
            .Some(async handler => {
                var result = handler.DynamicInvoke(request);
                return result switch {
                    Task<TResponse> task => await task,
                    TResponse response => response,
                    _ => throw new Exception("Unknown result " + result)
                };
            }).None(() => throw new Exception("No handler for request " + request.GetType()));

        public void Subscribe<T>(Action<T> action) where T : INotification
        {
            notificationHandlers.GetOrAdd(typeof(T), new List<Delegate>());
            notificationHandlers[typeof(T)].Add(action);
        }

        public void Handle<TRequest, TResponse>(Func<TRequest, TResponse> handler)
        {
            if (!requestHandlers.TryAdd(typeof(TRequest), handler)) {
                throw new Exception("Handler already registered for " + typeof(TRequest));
            }
        }

        public void Handle<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
        {
            if (!requestHandlers.TryAdd(typeof(TRequest), handler)) {
                throw new Exception("Handler already registered for " + typeof(TRequest));
            }
        }
    }
}
