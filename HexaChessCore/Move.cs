using System;
using System.Collections;
using System.Collections.Generic;

namespace HexaChessCore
{
  /// <summary>
  /// Singleton, collections of pieces' basic movement (Func[(int, int), (int, int)])
  /// </summary>
  public sealed class BasicMovingRule
  {
    private static BasicMovingRule? _instance;
    private BasicMovingRule() { }
    
    public static BasicMovingRule Instance => _instance ??= new BasicMovingRule();
    
    public bool BoundaryRule(Cell cell) => BoundaryRule(cell.ToTuple());
    public (int, int) Ortho00((int, int) tuple) => (tuple.Item1 + 1, tuple.Item2 + 1);
    public (int, int) Ortho02((int, int) tuple) => (tuple.Item1 + 1, tuple.Item2);
    public (int, int) Ortho04((int, int) tuple) => (tuple.Item1, tuple.Item2 - 1);
    public (int, int) Ortho06((int, int) tuple) => (tuple.Item1 - 1, tuple.Item2 - 1);
    public (int, int) Ortho08((int, int) tuple) => (tuple.Item1 - 1, tuple.Item2);
    public (int, int) Ortho10((int, int) tuple) => (tuple.Item1, tuple.Item2 + 1);
    
    public (int, int) Diag01((int, int) tuple) => (tuple.Item1 + 2, tuple.Item2 + 1);
    public (int, int) Diag03((int, int) tuple) => (tuple.Item1 + 1, tuple.Item2 - 1);
    public (int, int) Diag05((int, int) tuple) => (tuple.Item1 - 1, tuple.Item2 - 2);
    public (int, int) Diag07((int, int) tuple) => (tuple.Item1 - 2, tuple.Item2 - 1);
    public (int, int) Diag09((int, int) tuple) => (tuple.Item1 - 1, tuple.Item2 + 1);
    public (int, int) Diag11((int, int) tuple) => (tuple.Item1 + 1, tuple.Item2 + 2);

    public (int, int) Knight01L((int, int) tuple) => (tuple.Item1 + 3, tuple.Item2 + 2);
    public (int, int) Knight01R((int, int) tuple) => (tuple.Item1 + 3, tuple.Item2 + 1);
    public (int, int) Knight03L((int, int) tuple) => (tuple.Item1 + 2, tuple.Item2 - 1);
    public (int, int) Knight03R((int, int) tuple) => (tuple.Item1 + 1, tuple.Item2 - 2);
    public (int, int) Knight05L((int, int) tuple) => (tuple.Item1 - 1, tuple.Item2 - 3);
    public (int, int) Knight05R((int, int) tuple) => (tuple.Item1 - 2, tuple.Item2 - 3);
    public (int, int) Knight07L((int, int) tuple) => (tuple.Item1 - 3, tuple.Item2 - 2);
    public (int, int) Knight07R((int, int) tuple) => (tuple.Item1 - 3, tuple.Item2 - 1);
    public (int, int) Knight09L((int, int) tuple) => (tuple.Item1 - 2, tuple.Item2 + 1);
    public (int, int) Knight09R((int, int) tuple) => (tuple.Item1 - 1, tuple.Item2 + 2);
    public (int, int) Knight11L((int, int) tuple) => (tuple.Item1 + 1, tuple.Item2 + 3);
    public (int, int) Knight11R((int, int) tuple) => (tuple.Item1 + 2, tuple.Item2 + 3);

    public bool BoundaryRule((int, int) tuple)
    {
      return tuple.Item1 switch
      {
        0 when tuple.Item2 is >= 0 and <= 5 => true,
        1 when tuple.Item2 is >= 0 and <= 6 => true,
        2 when tuple.Item2 is >= 0 and <= 7 => true,
        3 when tuple.Item2 is >= 0 and <= 8 => true,
        4 when tuple.Item2 is >= 0 and <= 9 => true,
        5 when tuple.Item2 is >= 0 and <= 10 => true,
        6 when tuple.Item2 is >= 1 and <= 10 => true,
        7 when tuple.Item2 is >= 2 and <= 10 => true,
        8 when tuple.Item2 is >= 3 and <= 10 => true,
        9 when tuple.Item2 is >= 4 and <= 10 => true,
        10 when tuple.Item2 is >= 5 and <= 10 => true,
        _ => false
      };
    }
  }
  
  public class GetPossibleMove : IEnumerable<Cell>
  {
    private readonly Cell _initialPosition;
    private int _max;
    private Func<(int, int), (int, int)> _movingRule;
    private Predicate<(int, int)> _boundaryRule;

    public GetPossibleMove(Cell initialPosition, int max, Func<(int, int), (int, int)> movingRule, Predicate<(int, int)> boundaryRule)
    {
      _initialPosition = initialPosition;
      _max = max;
      _movingRule = movingRule;
      _boundaryRule = boundaryRule;
    }
    public GetPossibleMove(Cell initialPosition, int max, Func<(int, int), (int, int)> movingRule)
    {
      _initialPosition = initialPosition;
      _max = max;
      _movingRule = movingRule;
      _boundaryRule = BasicMovingRule.Instance.BoundaryRule;
    }
    

    IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator()
    {
      return GetEnumerator() as IEnumerator<Cell>;
    }

    public IEnumerator GetEnumerator()
    {
      return new GetPossibleMoveEnum(_initialPosition, _max, _movingRule, _boundaryRule);
    }
  }

  internal class GetPossibleMoveEnum : IEnumerator<Cell>
  {
    private readonly Cell _initialPosition;
    private Cell _currentCell;
    private int _max;
    private int _counter;
    private Func<(int, int), (int, int)> _movingRule;
    private Predicate<(int, int)> _boundaryRule;

    public GetPossibleMoveEnum(Cell initialPosition, int max, Func<(int, int), (int, int)> movingRule, Predicate<(int, int)> boundaryRule)
    {
      _initialPosition = initialPosition;
      _currentCell = initialPosition;
      _boundaryRule = boundaryRule;
      _max = max;
      _movingRule = movingRule;
      _counter = -1;
    }

    public Cell Current => _currentCell;

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
      _currentCell = _currentCell.Move(_movingRule);
      _counter++;
      return (_counter < _max) && (_boundaryRule(_currentCell.ToTuple()));
    }

    public void Reset()
    {
      _counter = -1;
      _currentCell = _initialPosition;
    }

    public void Dispose()
    {
    }
  }
}