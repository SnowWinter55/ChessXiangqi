# 03 - Triển Khai Logic Chi Tiết

## 📍 Khởi Tạo Bàn Cờ

### Cờ Vua (ChessBoard)

```csharp
ChessBoard.InitializeStandardPosition()

Vị trí chuẩn cờ vua:
┌───┬───┬───┬───┬───┬───┬───┬───┐
│ r │ n │ b │ q │ k │ b │ n │ r │ (Hàng 0: Quân đen)
├───┼───┼───┼───┼───┼───┼───┼───┤
│ p │ p │ p │ p │ p │ p │ p │ p │ (Hàng 1: Tốt đen)
├───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │ (Hàng 2-5: Trống)
├───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │
├───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │
├───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │
├───┼───┼───┼───┼───┼───┼───┼───┤
│ P │ P │ P │ P │ P │ P │ P │ P │ (Hàng 6: Tốt trắng)
├───┼───┼───┼───┼───┼───┼───┼───┤
│ R │ N │ B │ Q │ K │ B │ N │ R │ (Hàng 7: Quân trắng)
└───┴───┴───┴───┴───┴───┴───┴───┘
  0   1   2   3   4   5   6   7

Phương thức:
├── SetPieceAt(Position, new Pawn(Color.Black)) ×8  [Hàng 1]
├── SetPieceAt(..., Rook) [Góc hàng 0]
├── SetPieceAt(..., Knight) [Cạnh Rook]
├── SetPieceAt(..., Bishop) [Tiếp Knight]
├── SetPieceAt(..., Queen) [Trung tâm: d1/d8]
├── SetPieceAt(..., King) [Trung tâm: e1/e8]
└── [Lặp lại cho hàng 6-7 với Color.White]
```

---

### Cờ Tướng (XiangqiBoard)

```csharp
XiangqiBoard.InitializeStandardPosition()

Vị trí chuẩn cờ tướng (10×9):
┌───┬───┬───┬───┬───┬───┬───┬───┬───┐
│ 車│ 馬│ 象│ 士│ 將│ 士│ 象│ 馬│ 車│ (Hàng 0: Quân đỏ)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │   │ (Hàng 1: Trống)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│   │ 砲│   │   │   │   │   │ 砲│   │ (Hàng 2: Pháo)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│ 兵│   │ 兵│   │ 兵│   │ 兵│   │ 兵│ (Hàng 3: Tốt)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │   │ (Hàng 4: SÔNG)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │   │ (Hàng 5: SÔNG)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│ 兵│   │ 兵│   │ 兵│   │ 兵│   │ 兵│ (Hàng 6: Tốt)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│   │ 砲│   │   │   │   │   │ 砲│   │ (Hàng 7: Pháo)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│   │   │   │   │   │   │   │   │   │ (Hàng 8: Trống)
├───┼───┼───┼───┼───┼───┼───┼───┼───┤
│ 車│ 馬│ 象│ 士│ 將│ 士│ 象│ 馬│ 車│ (Hàng 9: Quân xanh)
└───┴───┴───┴───┴───┴───┴───┴───┴───┘
  0   1   2   3   4   5   6   7   8

CUNG ĐIỆN (Palace): Hàng 7-9, Cột 3-5 (cho quân xanh)
                    Hàng 0-2, Cột 3-5 (cho quân đỏ)

Phương thức:
├── SetPieceAt() × 32 quân cơ bản
├── Đặc biệt:
│   ├── Palace tại (row: 7-9, col: 3-5) cho White
│   ├── Palace tại (row: 0-2, col: 3-5) cho Black
│   └── River tại hàng 4-5
└── Kiểm tra constraints trong di chuyển
```

---

## 🎯 Xác Thực Nước Đi (Move Validation)

### Chuỗi Xác Thực

```
IsValidMove(board, move, currentTurn)
    ↓
1. Kiểm tra cơ bản:
   ├── From & To trong giới hạn board?
   ├── Piece tại From tồn tại?
   ├── Piece thuộc về currentTurn?
   └── To không chứa quân cùng màu?
    ↓
2. Kiểm tra quân cơ bản:
   ├── GetValidMoves() từ From position
   ├── To có trong danh sách không?
   └── Nếu không → Invalid
    ↓
3. Kiểm tra nước đặc biệt (tùy game):
   ├── En Passant (Cờ vua)?
   ├── Castling (Cờ vua)?
   ├── Promotion (Cờ vua)?
   ├── Palace restriction (Cờ tướng)?
   ├── River restriction (Cờ tướng)?
   └── Blocking piece (Cờ tướng)?
    ↓
4. Kiểm tra tính an toàn vua:
   ├── Clone board
   ├── MakeMove trên clone
   ├── IsCheck() sau nước đi?
   └── Nếu vua bị chiếu → Invalid
    ↓
5. Valid ✅
```

---

### Chi Tiết Từng Loại Quân

#### **King / General - Vua / Tướng**

```csharp
GetValidMoves():
├── Di chuyển 1 ô theo 8 hướng (King)
├── Bị giới hạn trong cung điện 3×3 (General - chỉ Xiangqi)
└── Không được di chuyển vào ô bị tấn công

Ví dụ (Cờ vua - King tại e1):
├── d1, e2, f1, f2 (nếu không bị chiếu)
└── d2, f2 cũng hợp lệ

Ví dụ (Cờ tướng - General tại e9):
├── Chỉ có thể trong cung điện (d8-f10)
├── Có thể: d8, d9, e10, f9, f10
└── KHÔNG: g9 (ngoài cung điện)
```

---

#### **Queen / Advisor - Hậu / Sĩ**

```csharp
Queen (Cờ vua):
├── Di chuyển theo 8 hướng (ngang, dọc, chéo)
├── Không giới hạn số ô
└── Bị chặn bởi quân khác

Advisor/Sĩ (Cờ tướng):
├── Di chuyển chéo 1 ô
├── Giới hạn trong cung điện 3×3
└── 5 vị trí hợp lệ: Tâm, 4 góc

Ví dụ (Advisor tại d9):
├── Có thể: c8, c10, e8, e10
└── KHÔNG: d8 (không chéo)
```

---

#### **Rook / Chariot - Xe / Xe**

```csharp
Rook (Cờ vua):
├── Di chuyển ngang hoặc dọc
├── Không giới hạn số ô
└── Bị chặn bởi quân khác

Chariot (Cờ tướng):
├── Di chuyển ngang hoặc dọc
├── Không giới hạn số ô
├── KHÔNG bị giới hạn bởi sông
└── Bị chặn bởi quân khác

Ví dụ (Chariot tại a0):
├── Có thể: b0, c0, d0, ... (ngang)
├── Có thể: a1, a2, ..., a9 (dọc, qua sông)
└── KHÔNG: bị quân khác chặn
```

---

#### **Bishop / Elephant - Tượng / Voi**

```csharp
Bishop (Cờ vua):
├── Di chuyển chéo
├── Không giới hạn số ô
├── Không thay đổi màu ô
└── Bị chặn bởi quân khác

Elephant (Cờ tướng - KHÁC BIỆT):
├── Di chuyển chéo 2 ô
├── PHẢI có quân ở giữa (trung điểm) để qua
├── KHÔNG được qua sông (ở nửa bàn của mình)
└── 4-7 vị trí hợp lệ tùy vị trí

Ví dụ (Elephant tại a0, quân White):
├── Có thể đến: c2 (với đường d1 trống)
├── KHÔNG: e4 (qua sông)
└── KHÔNG: c2 (nếu d1 có quân)
```

---

#### **Knight / Horse - Mã / Mã**

```csharp
Knight (Cờ vua):
├── Di chuyển hình chữ L: 2 ô + 1 ô
├── 8 vị trí hợp lệ tối đa
└── BỎ QUA quân ở giữa (không bị chặn)

Horse (Cờ tướng - KHÁC BIỆT):
├── Di chuyển hình chữ L: 2 ô + 1 ô (như Knight)
├── PHẢI không có quân ở vị trí giữa (hàng chặn)
├── 4-8 vị trí hợp lệ
└── VÍ DỤ: Horse tại d0 có thể đến:
    ├── b1 (nếu c0 trống)
    ├── f1 (nếu e0 trống)
    └── KHÔNG: vị trí nào có quân chặn

Khác biệt chính:
- Knight: Bỏ qua hàng chặn
- Horse: PHẢI trống hàng chặn (kiểm tra `IsBlocked()`)
```

---

#### **Pawn / Soldier - Tốt / Tốt**

```csharp
Pawn (Cờ vua):
├── Di chuyển 1 ô về phía địch (White: dòng giảm)
├── Lần đầu: có thể 2 ô (nếu lần đầu)
├── Ăn: chéo 1 ô về phía địch
├── En Passant: Ăn tây tạp (xem LastMove)
└── Promotion: Về tận cùng → phong cấp

Soldier (Cờ tướng):
├── Trước khi qua sông: Tối đa 1 ô về phía địch (hoặc chéo ngang)
├── Sau khi qua sông: 1 ô theo 4 hướng (lên, xuống, trái, phải)
├── Lưu ý: Tiến không lùi, chỉ ngang sau qua sông
└── KHÔNG phong cấp

Ví dụ (White Pawn tại e6):
├── Có thể: e5 (tiến 1)
├── Có thể: e4 (tiến 2, nếu lần đầu)
├── Có thể: d5, f5 (ăn)
└── En Passant: Nếu Black Pawn tại e4 vừa nhảy từ e2

Ví dụ (Black Soldier tại e3 chưa qua sông):
├── Có thể: e4, d3, f3 (tiến hoặc ngang)
├── Sau qua sông: d4, e5, f4, e3 (4 hướng)
└── KHÔNG lùi
```

---

#### **Cannon - Pháo**

```csharp
Cannon (Cờ tướng ĐẶC BIỆT):
├── Di chuyển KHÔNG quân:
│   ├── Ngang hoặc dọc
│   ├── Không giới hạn số ô
│   └── KHÔNG được gặp quân nào (khác Chariot)
│
└── Ăn PHẢI có quân:
    ├── Ngang hoặc dọc
    ├── PHẢI có đúng 1 quân ở giữa (trung điểm)
    ├── Ăn quân ở sau trung điểm
    └── Không thể ăn quân giữa

Ví dụ (Cannon tại a0):
├── Di chuyển: b0, c0, d0, ... (không quân)
├── Ăn: Nếu c0 có quân, có thể ăn d0
├── KHÔNG: Không thể ăn c0 nếu c0 có quân
└── KHÔNG: Không thể di chuyển c0 nếu c0 có quân
```

---

## 🔄 Thực Hiện Nước Đi (Make Move)

```csharp
board.MakeMove(move)
    ↓
1. Lấy quân từ vị trí From:
   piece = GetPieceAt(move.From)
    ↓
2. Xử lý nước đặc biệt:
   ├── En Passant (Cờ vua):
   │   └── RemoveEnPassantPawn()
   │
   ├── Castling (Cờ vua):
   │   └── MoveCastlingRook()
   │
   ├── Promotion (Cờ vua):
   │   └── ReplacePawnWithPromotedPiece()
   │
   └── (Cờ tướng không có nước đặc biệt)
    ↓
3. Di chuyển quân:
   ├── SetPieceAt(move.From, null)
   ├── SetPieceAt(move.To, piece)
   └── piece.UpdateMoved() (nếu cần, ví dụ King, Rook)
    ↓
4. Ghi lại nước đi:
   ├── _lastMove = move
   └── _pieceMoved.Add(piece, true) (cho castling check)
    ↓
5. Xong ✅
```

---

## ⏱️ Quản Lý Thời Gian (GameClock)

### Chu Kỳ Đồng Hồ

```csharp
Khởi tạo:
├── ClockSettings (VD: 300 giây, 0 increment)
├── TimeWhiteSeconds = 300
└── TimeBlackSeconds = 300

Lượt 1 (White di chuyển):
├── clock.StartTurn(Color.White)
│   ├── _currentPlayer = White
│   ├── _turnStartTime = DateTime.UtcNow
│   └── Timer.Start() [cập nhật mỗi 1 giây]
│
├── [Người chơi suy nghĩ 5 giây]
│
└── clock.StopTurn()
    ├── elapsed = DateTime.UtcNow - _turnStartTime
    ├── TimeWhiteSeconds -= (int)elapsed.TotalSeconds
    ├── TimeWhiteSeconds += IncrementSeconds (nếu có)
    ├── Timer.Stop()
    └── OnTimeUpdated event

Lượt 2 (Black di chuyển):
├── clock.StartTurn(Color.Black)
├── ...
└── clock.StopTurn()

Nếu hết giờ:
├── OnTimeOut event
├── Game Over: "[Trắng/Đen] hết giờ"
└── [Người kia thắng]
```

---

## 📜 Quản Lý Lịch Sử (HistoryManager & Command Pattern)

### Cấu Trúc Command

```csharp
interface ICommand:
├── Execute(): Thực hiện
└── Undo(): Hoàn tác

class MoveCommand implements ICommand:
├── _board: Bàn cờ
├── _move: Nước đi
├── _previousState: Trạng thái trước
│
├── Execute():
│   ├── Lưu trạng thái cũ
│   ├── board.MakeMove(_move)
│   └── Cập nhật Move.San nếu cần
│
└── Undo():
    ├── Khôi phục bàn từ trạng thái cũ
    └── board = _previousState.Clone()
```

### Undo/Redo Stack

```csharp
_undoStack = [Command1, Command2, Command3]  (top = mới nhất)
_redoStack = []

Action: Undo()
├── Pop từ _undoStack → Command3
├── Command3.Undo()
├── Push Command3 vào _redoStack
└── Result:
    _undoStack = [Command1, Command2]
    _redoStack = [Command3]

Action: Undo() lần 2
├── Pop từ _undoStack → Command2
├── Command2.Undo()
├── Push Command2 vào _redoStack
└── Result:
    _undoStack = [Command1]
    _redoStack = [Command3, Command2]

Action: Redo()
├── Pop từ _redoStack → Command2
├── Command2.Execute()
├── Push Command2 vào _undoStack
└── Result:
    _undoStack = [Command1, Command2]
    _redoStack = [Command3]

Lưu ý:
├── Khi thực hiện nước đi mới, _redoStack được xóa
└── Redo chỉ khôi phục nước đã Undo
```

---

## 🌳 Quản Lý Nhánh (BranchTracker - Tree Structure)

### Cấu Trúc Node

```csharp
class Node:
├── Move: Nước đi này
├── Parent: Nút cha
└── Children: List<Node> (nước thay thế)

BranchTracker:
├── _root: Node gốc (khi bàn trống)
├── _currentNode: Node hiện tại
└── _mainLine: Danh sách nước chính

Ví dụ cây nước:
        Root
         │
      Move 1: e4
         │
      Move 2: c5 ←── Nhánh 1: c5
         │      └─── Nhánh 2: e5
      Move 3: Nf3
         │
      Nhánh A: d6 ← Nhánh chính
      Nhánh B: e6
      Nhánh C: a6
```

### Các Phương Thức

```csharp
AddMove(move):
├── Kiểm tra move đã trong Children?
├── Nếu YES: _currentNode = node có move
├── Nếu NO: Tạo node mới, thêm vào Children
└── _currentNode = node mới

GoBack():
├── Nếu _currentNode.Parent: _currentNode = Parent
└── Return success

GetCurrentLine():
├── Duyệt từ _currentNode ngược về root
└── Return danh sách nước từ root → _currentNode

GetAvailableBranches():
├── Return _currentNode.Children
└── = Tất cả nước thay thế tại vị trí hiện tại

GoToMoveIndex(index):
├── Lấy nước thứ index từ _mainLine
├── Duyệt cây từ root → vị trí đó
└── _currentNode = node tại vị trí
```

---

## 📊 Kiểm Tra Kết Thúc Ván

### Checkmate - Chiếu Hết

```csharp
IsCheckmate(board, color):
    ├── 1. IsCheck(board, color)?
    │      ├── Vị trí Vua/Tướng
    │      └── Có quân đối phương tấn công?
    │
    └── 2. GetAllValidMoves(board, color).Count == 0?
           ├── Duyệt tất cả quân của color
           ├── Mỗi quân: GetValidMoves()
           ├── Filter out moves để Vua bị chiếu
           └── Nếu danh sách rỗng

Nếu cả 2 đều đúng:
└── CHECKMATE ✓
    └── "[Màu kia] thắng"

Ví dụ (Cờ vua):
├── Vua trắng bị chiếu từ Hậu đen
├── Trắng KHÔNG có nước:
│   ├── Không thể di chuyển vua ra ngoài
│   ├── Không thể chặn nước chiếu
│   ├── Không thể ăn Hậu chiếu
└── CHECKMATE → Đen thắng
```

---

### Stalemate - Hòa Cờ

```csharp
IsStalemate(board, color):
    ├── 1. IsCheck(board, color)?
    │      └── KHÔNG được bị chiếu
    │
    └── 2. GetAllValidMoves(board, color).Count == 0?
           └── Không có nước hợp lệ

Nếu 1 KHÔNG + 2 đúng:
└── STALEMATE ✓
    └── "Hòa cờ"

Ví dụ (Cờ vua):
├── Vua trắng KHÔNG bị chiếu
├── Trắng KHÔNG có nước:
│   └── Tất cả quân bị chặn
└── STALEMATE → Hòa
```

---

**Tiếp theo:** Đọc [04-LuồngChọnGame.md](04-LuồngChọnGame.md) để hiểu luồng menu.
