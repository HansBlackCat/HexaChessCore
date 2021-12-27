namespace HexaChessCore
{
  public class Game
  {
    private bool _isWhiteTurn;
    private Board _board;
      
    public Game(IBoardTemplate boardTemplate)
    {
      _board = new Board(boardTemplate);
      _isWhiteTurn = true;
    }
  }

  internal class BaseBoardTemplate
  {
    private static BaseBoardTemplate _instance;
    private TemplateFactory BaseBoard { get; }
    public IBoardTemplate GetTemplate => BaseBoard.GetTemplate!;

    private BaseBoardTemplate()
    {
      BaseBoard = new TemplateFactory();
      // Pawn
      for (int i = 0; i < 5; i++)
      {
        BaseBoard.AddPiece(new Pawn(new Cell(i, 4), PieceColor.White, SpecialRule.TwoInitialMove));
        BaseBoard.AddPiece(new Pawn(new Cell(i + 6, 6), PieceColor.Black, SpecialRule.TwoInitialMove));
      }
      for (int i = 0; i < 4; i++)
      {
        BaseBoard.AddPiece(new Pawn(new Cell(4, i), PieceColor.White, SpecialRule.TwoInitialMove));
        BaseBoard.AddPiece(new Pawn(new Cell(6, i + 6), PieceColor.Black, SpecialRule.TwoInitialMove));
      }

      // Bishop
      for (int i = 0; i < 3; i++)
      {
        BaseBoard.AddPiece(new Bishop(new Cell(i, i), PieceColor.White));
        BaseBoard.AddPiece(new Bishop(new Cell(i+8, i+8), PieceColor.Black));
      }
      
      // Rook
      BaseBoard.AddPiece(new Rook(new Cell(0, 3), PieceColor.White));
      BaseBoard.AddPiece(new Rook(new Cell(3, 0), PieceColor.White));
      BaseBoard.AddPiece(new Rook(new Cell(10, 7), PieceColor.Black));
      BaseBoard.AddPiece(new Rook(new Cell(7, 10), PieceColor.Black));
      
      // Knight
      BaseBoard.AddPiece(new Knight(new Cell(0, 2), PieceColor.White));
      BaseBoard.AddPiece(new Knight(new Cell(2, 0), PieceColor.White));
      BaseBoard.AddPiece(new Knight(new Cell(10, 8), PieceColor.Black));
      BaseBoard.AddPiece(new Knight(new Cell(8, 10), PieceColor.Black));
      
      // King
      BaseBoard.AddPiece(new King(new Cell(1, 0), PieceColor.White));
      BaseBoard.AddPiece(new King(new Cell(10, 9), PieceColor.Black));
      
      // Queen
      BaseBoard.AddPiece(new Queen(new Cell(0, 1), PieceColor.White));
      BaseBoard.AddPiece(new Queen(new Cell(9, 10), PieceColor.Black));
    }
    public static BaseBoardTemplate Instance => _instance ??= new BaseBoardTemplate();
  }
}