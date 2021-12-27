using System;
using System.Collections.Generic;

namespace HexaChessCore
{
  public enum PieceKind
  {
    None,
    King,
    Rook,
    Bishop,
    Queen,
    Knight,
    Pawn,
  }

  public enum PieceColor
  {
    None,
    White,
    Black,
  }

  [Flags]
  public enum SpecialRule
  {
    None,
    CanBeEnPassanted,
    TwoInitialMove,
    Promotion,
  }
  
  public interface IPiece
  {
    public PieceColor Color { get; }
    public PieceKind Kind { get; }

    public bool CanPromotion { get; }
    public bool IsHavingEnPassantFlag { get; }
    public bool IsHavingTwoInitialMoveFlag { get; }
    
    /// <summary>
    /// Get current location of this piece
    /// </summary>
    /// <returns>[2 coord] (RDiag, LDiag)</returns>
    public Cell GetCurrentLocation();

    public (int, int) ToTuple() =>
      GetCurrentLocation().ToTuple();

    
    /// <summary>
    /// Get possible move of the piece, IGNORE all other pieces
    /// </summary>
    /// <returns>Enumerable Collections of enumerable of piece moves on each direction</returns>
    public IEnumerable<IEnumerable<Cell>> GetPossibleMoveEnumerable();
    
    /// <summary>
    /// Move piece to certain cellvar m1 = BasicFunctionTest()
    /// </summary>
    public void Move(Cell cell);
  }

  public class Pawn : IPiece
  {
    private Cell _posititon;

    private SpecialRule _rule;

    public Pawn(Cell cell, PieceColor color, SpecialRule rule)
    {
      _rule = rule;
      _posititon = cell;
      Color = color;
    }
    
    public PieceColor Color { get; }
    public PieceKind Kind => PieceKind.Pawn;
    public bool CanPromotion => _rule.HasFlag(SpecialRule.Promotion);
    public bool IsHavingEnPassantFlag => _rule.HasFlag(SpecialRule.CanBeEnPassanted);
    public bool IsHavingTwoInitialMoveFlag => _rule.HasFlag(SpecialRule.TwoInitialMove);

    public Cell GetCurrentLocation()
    {
      return _posititon;
    }

    public IEnumerable<IEnumerable<Cell>> GetPossibleMoveEnumerable()
    {
      var max = _rule.HasFlag(SpecialRule.TwoInitialMove) ? 2 : 1;
      if (Color == PieceColor.White)
      {
        yield return new GetPossibleMove(_posititon, max, BasicMovingRule.Instance.Ortho00);
        yield return new GetPossibleMove(_posititon, 1, BasicMovingRule.Instance.Ortho02);
        yield return new GetPossibleMove(_posititon, 1, BasicMovingRule.Instance.Ortho10);
      }
      else if (Color == PieceColor.Black)
      {
        yield return new GetPossibleMove(_posititon, max, BasicMovingRule.Instance.Ortho06);
        yield return new GetPossibleMove(_posititon, 1, BasicMovingRule.Instance.Ortho04);
        yield return new GetPossibleMove(_posititon, 1, BasicMovingRule.Instance.Ortho08);
      }
      else
      {
        throw new Exception("Logic error, Color is undef");
      }
    }

    public void Move(Cell cell)
    {
      var isMovingTwoCell = !_posititon.IsPawnAdjacent(cell); 
      _posititon = cell;
      if (IsReadyToPromotion())
      {
        EnRule(SpecialRule.Promotion);
      }
      if (_rule.HasFlag(SpecialRule.CanBeEnPassanted))
      {
        UnRule(SpecialRule.CanBeEnPassanted);
      }
      if (_rule.HasFlag(SpecialRule.TwoInitialMove) && !IsInStartingPosition())
      {
        UnRule(SpecialRule.TwoInitialMove);
        if (isMovingTwoCell) EnRule(SpecialRule.CanBeEnPassanted);
      }
    }

    private void UnRule(SpecialRule rule)
    {
      _rule &= ~rule;
    }

    private void EnRule(SpecialRule rule)
    {
      _rule |= rule;
    }
    
    // Hexagonal Chess special rule
    private bool IsInStartingPosition()
    {
      switch (Color)
      {
        case PieceColor.White:
          return GetCurrentLocation().ToTuple() switch
          {
            (4, >= 0 and <= 4) => true,
            (>= 0 and <= 4, 4) => true,
            _ => false,
          };
        case PieceColor.Black:
          return GetCurrentLocation().ToTuple() switch
          {
            (6, >= 6 and <= 10) => true,
            (>= 6 and <= 10, 6) => true,
            _ => false,
          };
        default:
          throw new Exception("Logic error, Color is undef");
      }
    }
    
    // Promotion Check
    private bool IsReadyToPromotion()
    {
      switch (Color)
      {
        case PieceColor.White:
          return GetCurrentLocation().ToTuple() switch
          {
            (10, _) => true,
            (_, 10) => true,
            _ => false,
          };
        case PieceColor.Black:
          return GetCurrentLocation().ToTuple() switch
          {
            (0, _) => true,
            (_, 0) => true,
            _ => false,
          };
        default:
          throw new Exception("Logic error, Color is undef");
      }
    }
  }

  public class King : IPiece
  {
    private Cell _position;

    public King(Cell position, PieceColor color)
    {
      _position = position;
      Color = color;
    }
    
    public PieceColor Color { get; }
    public PieceKind Kind => PieceKind.King;
    public bool CanPromotion => false;
    public bool IsHavingEnPassantFlag => false;
    public bool IsHavingTwoInitialMoveFlag => false;
    public Cell GetCurrentLocation() => _position;

    public IEnumerable<IEnumerable<Cell>> GetPossibleMoveEnumerable()
    {
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Ortho00);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Ortho02);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Ortho04);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Ortho06);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Ortho08);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Ortho10);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Diag01);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Diag03);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Diag05);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Diag07);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Diag09);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Diag11);
    }

    public void Move(Cell cell) => _position = cell;
  }

  public class Rook : IPiece
  {
    private Cell _position;

    public Rook(Cell position, PieceColor color)
    {
      _position = position;
      Color = color;
    }

    public PieceColor Color { get; }
    public PieceKind Kind => PieceKind.Rook;
    public bool CanPromotion => false;
    public bool IsHavingEnPassantFlag => false;
    public bool IsHavingTwoInitialMoveFlag => false;

    public Cell GetCurrentLocation()
    {
      return _position;
    }

    public IEnumerable<IEnumerable<Cell>> GetPossibleMoveEnumerable()
    {
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho00);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho02);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho04);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho06);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho08);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho10);
    }

    public void Move(Cell cell)
    {
      _position = cell;
    }
  }

  public class Bishop : IPiece
  {
    private Cell _position;
    
    public Bishop(Cell position, PieceColor color)
    {
      _position = position;
      Color = color;
    }
    public PieceColor Color { get; }
    public PieceKind Kind => PieceKind.Bishop;
    public bool CanPromotion => false;
    public bool IsHavingEnPassantFlag => false;
    public bool IsHavingTwoInitialMoveFlag => false;
    public Cell GetCurrentLocation() => _position;

    public IEnumerable<IEnumerable<Cell>> GetPossibleMoveEnumerable()
    {
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag01);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag03);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag05);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag07);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag09);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag11);
    }

    public void Move(Cell cell)
    {
      _position = cell;
    }
  }

  public class Queen : IPiece
  {
    private Cell _position;
    
    public Queen(Cell position, PieceColor color)
    {
      _position = position;
      Color = color;
    }
    public PieceColor Color { get; }
    public PieceKind Kind => PieceKind.Queen;
    public bool CanPromotion => false;
    public bool IsHavingEnPassantFlag => false;
    public bool IsHavingTwoInitialMoveFlag => false;
    public Cell GetCurrentLocation() => _position;

    public IEnumerable<IEnumerable<Cell>> GetPossibleMoveEnumerable()
    {
      
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho00);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho02);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho04);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho06);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho08);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Ortho10);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag01);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag03);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag05);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag07);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag09);
      yield return new GetPossibleMove(_position, Int32.MaxValue, BasicMovingRule.Instance.Diag11);
    }

    public void Move(Cell cell)
    {
      _position = cell;
    }
  }

  public class Knight : IPiece
  {
    private Cell _position;
    
    public Knight(Cell position, PieceColor color)
    {
      _position = position;
      Color = color;
    }
    public PieceColor Color { get; }
    public PieceKind Kind => PieceKind.Knight;
    public bool CanPromotion => false;
    public bool IsHavingEnPassantFlag => false;
    public bool IsHavingTwoInitialMoveFlag => false;
    public Cell GetCurrentLocation() => _position;

    public IEnumerable<IEnumerable<Cell>> GetPossibleMoveEnumerable()
    {
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight01L);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight01R);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight03L);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight03R);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight05L);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight05R);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight07L);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight07R);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight09L);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight09R);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight11L);
      yield return new GetPossibleMove(_position, 1, BasicMovingRule.Instance.Knight11R);
    }

    public void Move(Cell cell)
    {
      _position = cell;
    }
  }
  
}