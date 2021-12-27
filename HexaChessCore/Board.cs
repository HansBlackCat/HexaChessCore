using System;
using System.Collections.Generic;
using System.Linq;

namespace HexaChessCore
{
  public enum CellColor
  {
    White,
    Gray,
    Black,
  }
  
  public readonly struct Cell
  {
    public readonly int RDiag;
    public readonly int LDiag;

    public Cell(int rDiag, int lDiag)
    {
      RDiag = rDiag;
      LDiag = lDiag;
    }

    public Cell((int, int) tuple)
    {
      RDiag = tuple.Item1;
      LDiag = tuple.Item2;
    }

    public static Cell Empty()
    {
      return new Cell(-1, -1);
    }

    public bool IsEmpty()
    {
      return RDiag == -1 || LDiag == -1;
    }

    public (int, int) ToTuple()
    {
      return (RDiag, LDiag);
    }

    public Cell Move(Func<(int, int), (int, int)> movingRule)
    {
      var valueTuple = movingRule((RDiag, LDiag));
      return new Cell(valueTuple);
    }

    public bool IsPawnAdjacent(Cell cell)
    {
      var list = new[]
      {
        new Cell(RDiag + 1, LDiag + 1),
        new Cell(RDiag + 1, LDiag),
        new Cell(RDiag, LDiag - 1),
        new Cell(RDiag - 1, LDiag - 1),
        new Cell(RDiag - 1, LDiag),
        new Cell(RDiag, LDiag + 1),
      };
      return list.Contains(cell);
    }

    public bool IsKingAdjacent(Cell cell)
    {
      var plusList = new[]
      {
        new Cell(RDiag + 2, LDiag + 1),
        new Cell(RDiag + 1, LDiag - 1),
        new Cell(RDiag - 1, LDiag - 2),
        new Cell(RDiag - 2, LDiag - 1),
        new Cell(RDiag - 1, LDiag + 1),
        new Cell(RDiag + 1, LDiag + 2),
      };
      return IsPawnAdjacent(cell) || plusList.Contains(cell);
    }

    #region Overloading
    public override int GetHashCode()
    {
      return RDiag * 100 + LDiag;
    }

    public override bool Equals(object? obj)
    {
      return this.GetHashCode() == ((Cell)(obj ?? Cell.Empty())).GetHashCode();
    }

    public static bool operator ==(Cell? c1, Cell? c2)
    {
      return c1?.GetHashCode() == c2?.GetHashCode();
    }

    public static bool operator !=(Cell? c1, Cell? c2)
    {
      return !(c1 == c2);
    }

    public override string ToString() => $"{RDiag} {LDiag}";
    #endregion
  }

  
  public class Board
  {
    private Dictionary<(int, int), IPiece?> _chessBoard;
    private Dictionary<(int, int), List<Cell>> _movable;
    private List<(PieceColor, PieceKind)> _grave = new List<(PieceColor, PieceKind)>();
    public IEnumerable<IPiece?> GetPiecesEnum => _chessBoard.Values.AsEnumerable();
    public List<Cell> GetMovableOfCell((int, int) tuple) => _movable[tuple];
    public List<Cell> GetMovableOfCell(int rDiag, int lDiag) => _movable[(rDiag, lDiag)];
    public Dictionary<(int, int), List<Cell>> GetMovable => _movable;
    public List<(PieceColor, PieceKind)> GetGrave => _grave;

    public Board(IBoardTemplate template)
    {
      _chessBoard = template.GetBoard();
      _movable = template.GetMovable();
      CalculateMovable();
    }

    public IPiece? GetPieceByIndex((int, int) tuple) => _chessBoard[(tuple.Item1, tuple.Item2)];
    public IPiece? GetPieceByIndex(int rDiag, int lDiag) => _chessBoard[(rDiag, lDiag)];

    // TODO should kill action here?
    public bool MakeMove(Cell from, PieceColor fromColor ,Cell to)
    {
      if (_chessBoard[from.ToTuple()] is null || _chessBoard[from.ToTuple()]!.Color != fromColor) return false;
      if (!_movable[from.ToTuple()].Contains(to)) return false;
      
      // TODO reference?
      _chessBoard[from.ToTuple()]!.Move(to);
      // TODO make rewind
      if (_chessBoard[to.ToTuple()] is not null)
      {
        _grave.Add((_chessBoard[to.ToTuple()]!.Color, _chessBoard[to.ToTuple()]!.Kind));
      }
      _chessBoard[to.ToTuple()] = _chessBoard[from.ToTuple()];
      _chessBoard[from.ToTuple()] = null;
      CalculateMovable();
      return true;
    }
    
    public void CalculateMovable()
    {
      var valueEnum =
        from piece in _chessBoard
        where piece.Value is not null
        select (piece.Key, piece.Value.Color, piece.Value.Kind, piece.Value.GetPossibleMoveEnumerable());

      var absolutePin = new List<(int, int)>();
      var trackKing = new List<((int, int), PieceColor)>();
      var trackPawn = new List<((int, int), PieceColor)>();
      absolutePin.Clear();
      trackKing.Clear();
      trackPawn.Clear();

      ClearMovable();
      UpdateMovable();
      ManagePinned();
      ManageKing();
      ManagePawn();

      #region Caculator
      void ClearMovable()
      {
        foreach (var item in _movable)
        {
          item.Value.Clear();
        }
      }

      void UpdateMovable()
      {
        foreach (var pairs in valueEnum)
        {
          UpdateCellMovable(pairs);
        }
      }

      void UpdateCellMovable(((int, int), PieceColor, PieceKind, IEnumerable<IEnumerable<Cell>>) pairs)
      {
        ((int, int) key, PieceColor color, PieceKind kind, IEnumerable<IEnumerable<Cell>> enums) = pairs; 
        // Track King
        if (kind == PieceKind.King) trackKing.Add((key, color));
        // Track Pawn
        if (kind == PieceKind.Pawn) trackPawn.Add((key, color));
        foreach (var dir in enums)
        {
          var isOverIterate = false;
          var pinCheck = Cell.Empty();
          foreach (var cell in dir)
          {
            var isNextExist = GetPieceByIndex(cell.ToTuple()) is not null;
            if (!isOverIterate && !isNextExist)
            {
              _movable[key].Add(cell);
              continue;
            }
            // Case ally
            // Exit loop
            if (!isOverIterate && GetPieceByIndex(cell.ToTuple())!.Color == color)
            {
              break;
            }
            // Case foe
            // Should calculate pin
            if (!isOverIterate && GetPieceByIndex(cell.ToTuple())!.Color != color)
            {
              _movable[key].Add(cell);
              pinCheck = cell;
              isOverIterate = true;
              continue;
            }
            // Over-iterating but empty 
            if (isOverIterate && !isNextExist)
            {
              continue;
            }
            // Over-iterating && now pointing foe king
            if (isOverIterate && GetPieceByIndex(cell.ToTuple())!.Color != color &&
                GetPieceByIndex(cell.ToTuple())!.Kind == PieceKind.King)
            {
              if (pinCheck.IsEmpty()) throw new Exception("Trying to add empty cell to Pins");
              absolutePin.Add(pinCheck.ToTuple());
            }
            // Over-iterating && pointing else
            else
            {
              break;
            }
          }
        }
      }

      void ManagePinned()
      {
        foreach (var tuple in absolutePin)
        {
          _movable[tuple].Clear();
        }
      }

      void ManageKing()
      {
        foreach (var tuple in trackKing)
        {
          var (key, color) = tuple;
          // TODO Should test
          var flattenedEnemyMovableIterate =
            from value in _movable
            from item in value.Value
            where _chessBoard[value.Key].Color != color
            where item.IsKingAdjacent(new Cell(key))
            select item;
          foreach (var cell in flattenedEnemyMovableIterate)
          {
            _movable[key].Remove(cell);
          }
        }
      }

      void ManagePawn()
      {
        foreach (var (pair, color) in trackPawn)
        {
          var (i, j) = pair;
          var whitePawnSpecialMoves = new[]
          {
            (i + 1, j), (i, j + 1)
          };
          var blackPawnSpecialMoves = new[]
          {
            (i - 1, j), (i, j - 1)
          };
          
          if (color == PieceColor.White)
          {
            // Iterate 2
            foreach (var tup in whitePawnSpecialMoves)
            {
              if (_chessBoard[tup] is not null && _chessBoard[tup]!.Color == PieceColor.Black)
              {
                continue;
              }
              // EnPassant Checking
              var enpassantFoeLocation = (tup.Item1 - 1, tup.Item2 - 1);
              if (!BasicMovingRule.Instance.BoundaryRule(enpassantFoeLocation)) continue;
              if (_chessBoard[enpassantFoeLocation] is not null && _chessBoard[enpassantFoeLocation]!.IsHavingEnPassantFlag)
              {
                continue;
              }

              _movable[pair].Remove(new Cell(tup));
            }
          }
          else if (color == PieceColor.Black)
          {
            // Iterate 2
            foreach (var tup in blackPawnSpecialMoves)
            {
              if (_chessBoard[tup] is not null && _chessBoard[tup]!.Color == PieceColor.White)
              {
                continue;
              }
              // EnPassant Checking
              var enpassantFoeLocation = (tup.Item1 + 1, tup.Item2 + 1);
              if (!BasicMovingRule.Instance.BoundaryRule(enpassantFoeLocation)) continue;
              if (_chessBoard[enpassantFoeLocation] is not null && _chessBoard[enpassantFoeLocation]!.IsHavingEnPassantFlag)
              {
                continue;
              }

              _movable[pair].Remove(new Cell(tup));
            }
          }
          else
          {
            throw new Exception("Logic error, Color is undef");
          }
        }
      }
      #endregion
    }
  }
  
  public interface IBoardTemplate
  {
    Dictionary<(int, int), IPiece?> GetBoard();
    Dictionary<(int, int), List<Cell>> GetMovable();

    void AddBoard(IPiece piece);
  }
  

  public class TemplateFactory
  {
    public IBoardTemplate? GetTemplate { get; }

    public TemplateFactory()
    {
      GetTemplate = new Template();
    }

    public void AddPiece(IPiece piece) =>
      GetTemplate!.AddBoard(piece);
    
    
    class Template : IBoardTemplate
    {
      // TODO Need singleton?
      // private Template? _instance;
      public Dictionary<(int, int), IPiece?> GetBoard() => _template;
      public Dictionary<(int, int), List<Cell>> GetMovable() => _movable;

      public Template()
      {
        // [(0,0),(0,1),...,(10,10)]
        var list =
          from rDiag in Enumerable.Range(0, 11)
          from lDiag in Enumerable.Range(0, 11)
          where BasicMovingRule.Instance.BoundaryRule((rDiag, lDiag))
          select (rDiag, lDiag);

        var count = 0;
        foreach (var item in list)
        {
          count++;
          GetBoard().Add(item, null);
          GetMovable().Add(item, new List<Cell>());
          GetMovable()[item].Clear();
        }

        if (count != 91) throw new Exception("Implementing Template error");
      }

      // TODO Need singleton?
      // public Template Instance => _instance ??= new Template();
      private readonly Dictionary<(int, int), IPiece?> _template = new Dictionary<(int, int), IPiece?>();
      private readonly Dictionary<(int, int), List<Cell>> _movable = new Dictionary<(int, int), List<Cell>>();

      public void AddBoard(IPiece piece)
      {
        if (GetBoard()[piece.ToTuple()] is not null)
        {
          throw new Exception("Try to add to location where sth already exists");
        }
        GetBoard()[piece.ToTuple()] = piece;
      }

      public void AddNull((int, int) tuple)
      {
        GetBoard()[tuple] = null;
      }
    }

  }
  
}