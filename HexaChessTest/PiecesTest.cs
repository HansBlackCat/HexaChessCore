using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using HexaChessCore;

namespace HexaChessTest;

public class PiecesTest
{
  private void CollectionPreciseTest(Cell position, ref HashSet<Cell> set, ref Board board)
  {
    Assert.Equal(set.Count, board.GetMovableOfCell(position.ToTuple()).Count);
    Assert.True(board.GetMovableOfCell(position.ToTuple()).ToHashSet().SetEquals(set));
  }
  
  [Fact]
  public void BasicFunctionTest()
  {
    var c1 = new Cell(3, 10);
    var c2 = new Cell(3, 10);
    Assert.True(c1 == c2);

    var s1 = SpecialRule.CanBeEnPassanted | SpecialRule.TwoInitialMove;
    Assert.True(s1.HasFlag(SpecialRule.CanBeEnPassanted));

    s1 &= ~SpecialRule.CanBeEnPassanted;
    Assert.False(s1.HasFlag(SpecialRule.CanBeEnPassanted));
    Assert.True(s1.HasFlag(SpecialRule.TwoInitialMove));
  }

  [Fact]
  public void MovingClassTest()
  {
    var c1 = new Cell(5, 5);
    var m1 = new GetPossibleMove(c1, 5, 
      tup => (tup.Item1 + 1, tup.Item2 + 1), 
      tup => tup.Item1 < 10);
    var listOri = m1.Where(tup => tup.RDiag != 8).ToList();
    var list = m1.TakeWhile(tup => tup.RDiag != 8).ToList();

    Assert.True(listOri.Count == 3);
    Assert.True(list.Count == 2);
  }

  [Fact]
  public void BasicMoveTest()
  {
    var c1 = new Cell(5, 5);
    var m1 = new GetPossibleMove(c1, 2, BasicMovingRule.Instance.Ortho00, tuple => true);
    Assert.True(m1.ToList().Count == 2);
  }

  [Fact]
  public void BoardComposeTest()
  {
    var template = new TemplateFactory();
    template.AddPiece(new Rook(new Cell(5, 5), PieceColor.White));
    
    var board = new Board(template.GetTemplate);
    var e = board.GetPieceByIndex(5, 5)?.GetPossibleMoveEnumerable();

    foreach (var dir in e?.Select((val, i) => (val, i))!)
    {
      Assert.Equal(5, dir.val.Count());
    }
  }

  [Fact]
  public void BoardEnumerable()
  {
    var template = new TemplateFactory();
    template.AddPiece(new Rook(new Cell(5, 5), PieceColor.White));
    
    var board = new Board(template.GetTemplate!);
    var en = board.GetPiecesEnum;

    foreach (var item in en)
    {
      Debug.WriteLine(item is null ? "NULL!" : $"{item.ToTuple()} -> {item.Color}");
    }
    Assert.True(true);
  }

  [Fact]
  public void ErrorHandlingTest()
  {
    var template = new TemplateFactory();
    template.AddPiece(new Rook(new Cell(5, 5), PieceColor.White));
    template.AddPiece(new Pawn(new Cell(4, 5), PieceColor.White, SpecialRule.None));
    template.AddPiece(new Pawn(new Cell(5, 8), PieceColor.Black, SpecialRule.None));
    Assert.Throws<Exception>(() =>
    {
      template.AddPiece(new Pawn(new Cell(5, 8), PieceColor.White, SpecialRule.None));
    });
  }

  [Fact]
  public void SimpleMovableTest01()
  {
    var template = new TemplateFactory();
    template.AddPiece(new Rook(new Cell(5, 5), PieceColor.White));
    template.AddPiece(new Pawn(new Cell(6, 5), PieceColor.Black, SpecialRule.None));
    template.AddPiece(new Pawn(new Cell(5, 8), PieceColor.Black, SpecialRule.None));
    var board = new Board(template.GetTemplate!);
    
    Assert.Equal(24, board.GetMovableOfCell(5, 5).Count());
    Assert.Empty(board.GetMovableOfCell(0, 0));
    Assert.Equal(2, board.GetMovableOfCell(6, 5).Count());
  }

  [Fact]
  public void EnPassantMovingCheck()
  {
    var template = new TemplateFactory();
    // White Pawn
    template.AddPiece(new Pawn(new Cell(4, 4), PieceColor.White, SpecialRule.TwoInitialMove));
    // Black Pawn which can be En-passant-ed
    template.AddPiece(new Pawn(new Cell(6, 7), PieceColor.Black, SpecialRule.TwoInitialMove));
    // Black Pawn in initialize state
    template.AddPiece(new Pawn(new Cell(7, 6), PieceColor.Black, SpecialRule.TwoInitialMove));
    var board = new Board(template.GetTemplate!);
    
    // White -> Black -> Black for test
    // TODO Make Test Pack?

    var PossibleIn = new HashSet<Cell> {new Cell(5, 5), new Cell(6, 6)};
    Assert.Equal(PossibleIn.Count(), board.GetMovableOfCell(4,4).Count());
    Assert.True(board.GetMovableOfCell(4,4).ToHashSet().SetEquals(PossibleIn));

    // White Moves
    Assert.True(board.MakeMove(new Cell(4, 4), PieceColor.White, new Cell(5, 5)));
    Assert.False(board.GetPieceByIndex(5, 5)!.IsHavingEnPassantFlag);
    Assert.False(board.GetPieceByIndex(5,5)!.CanPromotion);
    Assert.False(board.GetPieceByIndex(5,5)!.IsHavingTwoInitialMoveFlag);
    
    PossibleIn.Clear();
    PossibleIn.Add(new Cell(6, 5));
    PossibleIn.Add(new Cell(5, 4));
    Assert.Equal(PossibleIn.Count(), board.GetMovableOfCell(7,6).Count());
    Assert.True(board.GetMovableOfCell(7,6).ToHashSet().SetEquals(PossibleIn));
    
    // Back Moves
    Assert.True(board.MakeMove(new Cell(7, 6), PieceColor.Black, new Cell(6, 5)));
    Assert.False(board.GetPieceByIndex(6, 5)!.IsHavingEnPassantFlag);
    Assert.False(board.GetPieceByIndex(6,5)!.CanPromotion);
    Assert.False(board.GetPieceByIndex(6,5)!.IsHavingTwoInitialMoveFlag);
    
    Assert.False(board.GetPieceByIndex(6,7)!.IsHavingEnPassantFlag);
    // Black Moves
    Assert.True(board.MakeMove(new Cell(6, 7), PieceColor.Black, new Cell(4, 5)));
    Assert.False(board.GetPieceByIndex(4,5)!.CanPromotion);
    Assert.False(board.GetPieceByIndex(4,5)!.IsHavingTwoInitialMoveFlag);
    Assert.True(board.GetPieceByIndex(4, 5)!.IsHavingEnPassantFlag);
    
    PossibleIn.Clear();
    PossibleIn.Add(new Cell(6, 6));
    PossibleIn.Add(new Cell(6, 5));
    PossibleIn.Add(new Cell(5, 6));
    Assert.Equal(PossibleIn.Count(), board.GetMovableOfCell(5,5).Count());
    Assert.True(board.GetMovableOfCell(5,5).ToHashSet().SetEquals(PossibleIn));
  }

  [Fact]
  public void HexagonalChessSpecialDoubleMoveCheck()
  {
    var template = new TemplateFactory();
    template.AddPiece(new Pawn(new Cell(10, 6), PieceColor.Black, SpecialRule.TwoInitialMove));
    template.AddPiece(new Pawn(new Cell(9, 6), PieceColor.White, SpecialRule.None));
    template.AddPiece(new Pawn(new Cell(6, 9), PieceColor.White, SpecialRule.None));
    var board = new Board(template.GetTemplate!);
    
    // Catch White
    Assert.True(board.MakeMove(new Cell(10, 6), PieceColor.Black, new Cell(9,6)));
    Assert.Equal(PieceColor.Black, board.GetPieceByIndex(9, 6)!.Color);
    Assert.True(board.GetPieceByIndex(9, 6)!.IsHavingTwoInitialMoveFlag);
    var PossibleIn = new HashSet<Cell>()
    {
      new Cell(8,5),
      new Cell(7,4),
    };
    CollectionPreciseTest(new Cell(9,6), ref PossibleIn, ref board);
    Assert.Collection(board.GetGrave,
      item => Assert.True(item.Item1 == PieceColor.White && item.Item2 == PieceKind.Pawn));
  }

  [Fact]
  public void KingPinAndShadingTest()
  {
    var template = new TemplateFactory();
    template.AddPiece(new King(new Cell(0, 4), PieceColor.Black));
    // Should absolute pinned
    template.AddPiece(new Bishop(new Cell(1, 4), PieceColor.Black));
    // Should pin bishop
    template.AddPiece(new Rook(new Cell(9, 4), PieceColor.White));
    // Should shade king's move
    template.AddPiece(new Rook(new Cell(5, 10), PieceColor.White));
    var board = new Board(template.GetTemplate!);

    var PossibleIn = new HashSet<Cell>();
    PossibleIn.Clear();
    PossibleIn.Add(new Cell(1, 5));
    PossibleIn.Add(new Cell(0, 3));
    PossibleIn.Add(new Cell(2, 5));
    PossibleIn.Add(new Cell(1, 3));
    CollectionPreciseTest(new Cell(0, 4), ref PossibleIn, ref board);
    
    PossibleIn.Clear();
    CollectionPreciseTest(new Cell(1, 4), ref PossibleIn, ref board);
  }
  
  // TODO Check & CheckMate
  // TODO 
}