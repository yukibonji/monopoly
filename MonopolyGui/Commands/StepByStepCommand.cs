using Microsoft.FSharp.Core;
using Monopoly;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonopolyGui
{
    /// <summary>
    /// Plays a game one move at a time.
    /// </summary>
    [ImplementPropertyChanged]
    public class StepByStepCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private List<MovementEvent>.Enumerator gameHistory;
        private readonly IDictionary<String, BoardPosition> results;
        public String Message { get; set; }

        public Boolean CanExecute(Object parameter) { return true; }
        public StepByStepCommand(IDictionary<String, BoardPosition> results)
        {
            this.results = results;

            var auditRecord = new List<MovementEvent>();
            var controller = new Controller();
            controller.OnMoved += (o, e) => auditRecord.Add(e);

            Task.Run(() => controller.PlayGame(5000))
                .ContinueWith(t => { gameHistory = auditRecord.GetEnumerator(); });
        }

        public void Execute(Object parameter)
        {
            DeselectCurrentPosition(gameHistory.Current);
            gameHistory.MoveNext();

            var state = gameHistory.Current;
            var movingToName = state.MovingTo.ToString();

            if (state.Rolled.IsSome())
                Message = String.Format("Rolled {0} & {1}. {2} {3} (Doubles: {4})", state.Rolled.Value.Item1, state.Rolled.Value.Item2, state.MovementType, movingToName, state.DoubleCount);
            else
                Message = String.Format("{0} {1}", state.MovementType, movingToName);
            SelectNextPosition(movingToName);
        }

        private void DeselectCurrentPosition(MovementEvent state)
        {
            var gameHasStarted = (state != null);
            if (gameHasStarted)
                results[state.MovingTo.ToString()].Deselect();
        }
        private void SelectNextPosition(String movingToName)
        {
            var result = results[movingToName];
            result.Increment();
            result.Select();
        }
    }

    public static class FSOptionEx
    {
        public static bool IsSome<T>(this FSharpOption<T> option)
        {
            return FSharpOption<T>.get_IsSome(option);
        }
    }
}