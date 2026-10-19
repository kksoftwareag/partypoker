using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace PlanningPoker.Web.Services
{
    public record Toast(Guid Id, string Title, string Text);

    /// <summary>
    /// Short-lived, non-blocking notifications (e.g. "Anna ist beigetreten").
    /// Scoped per circuit so every connected player sees the same room events.
    /// </summary>
    public class ToastService : IDisposable, IAsyncDisposable
    {
        private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(4);

        private readonly object gate = new();
        private readonly List<Toast> toasts = [];
        private readonly Dictionary<Guid, Timer> timers = [];

        public event EventHandler Changed;

        public IReadOnlyList<Toast> Toasts
        {
            get
            {
                lock (this.gate)
                {
                    return this.toasts.ToArray();
                }
            }
        }

        public void Show(string title, string text = "")
        {
            var toast = new Toast(Guid.NewGuid(), title, text);
            var timer = new Timer(Lifetime.TotalMilliseconds) { AutoReset = false, Enabled = true };
            timer.Elapsed += (s, e) => Task.Run(() =>
            {
                this.Dismiss(toast.Id);
                timer.Dispose();
            });

            lock (this.gate)
            {
                this.toasts.Add(toast);
                this.timers[toast.Id] = timer;

                if (this.toasts.Count > 3)
                {
                    this.Remove(this.toasts[0].Id);
                }
            }

            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        public void Dismiss(Guid id)
        {
            lock (this.gate)
            {
                if (this.Remove(id) == false)
                {
                    return;
                }
            }

            this.Changed?.Invoke(this, EventArgs.Empty);
        }

        private bool Remove(Guid id)
        {
            if (this.timers.Remove(id, out var timer))
            {
                timer.Dispose();
            }

            var index = this.toasts.FindIndex(t => t.Id == id);
            if (index < 0)
            {
                return false;
            }

            this.toasts.RemoveAt(index);
            return true;
        }

        public ValueTask DisposeAsync()
        {
            this.Dispose();
            return ValueTask.CompletedTask;
        }

        public void Dispose()
        {
            lock (this.gate)
            {
                foreach (var timer in this.timers.Values)
                {
                    timer.Dispose();
                }

                this.timers.Clear();
                this.toasts.Clear();
            }
        }
    }
}
