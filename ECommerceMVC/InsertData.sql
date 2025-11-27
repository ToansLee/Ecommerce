-- =============================================
-- Script INSERT dữ liệu cho Food Ordering Database
-- Ngày tạo: 2025-11-25
-- =============================================

USE [FoodOrderingDB]
GO

-- =============================================
-- 1. INSERT CUSTOMERS (Sellers)
-- =============================================
SET IDENTITY_INSERT [dbo].[Customers] ON
GO

INSERT INTO [dbo].[Customers] (Id, Email, FullName, Phone, PasswordHash, Role, CreatedAt, IsActive)
VALUES 
(101, 'buffet.poseidon@restaurant.vn', 'Buffet Poseidon', '0912345678', 'AQAAAAEAACcQAAAAEK8h1234567890abcdefghijklmnopqrstuvwxyz', 'Seller', GETDATE(), 1),
(102, 'lau99.hoangmai@restaurant.vn', 'Lẩu 99 Hoàng Mai', '0988776655', 'AQAAAAEAACcQAAAAEK8h1234567890abcdefghijklmnopqrstuvwxyz', 'Seller', GETDATE(), 1),
(103, 'calavong@restaurant.vn', 'Quán Cá Lã Vọng', '0933221100', 'AQAAAAEAACcQAAAAEK8h1234567890abcdefghijklmnopqrstuvwxyz', 'Seller', GETDATE(), 1),
(104, 'haisannambien@restaurant.vn', 'Nhà Hàng Hải Sản Nam Biển', '0909988776', 'AQAAAAEAACcQAAAAEK8h1234567890abcdefghijklmnopqrstuvwxyz', 'Seller', GETDATE(), 1),
(105, 'phoxua@restaurant.vn', 'Nhà hàng Phở Xưa', '0977554433', 'AQAAAAEAACcQAAAAEK8h1234567890abcdefghijklmnopqrstuvwxyz', 'Seller', GETDATE(), 1),
(106, 'ocsaigon@restaurant.vn', 'Quán Ốc Sài Gòn', '0966112200', 'AQAAAAEAACcQAAAAEK8h1234567890abcdefghijklmnopqrstuvwxyz', 'Seller', GETDATE(), 1)

SET IDENTITY_INSERT [dbo].[Customers] OFF
GO

-- =============================================
-- 2. INSERT MENU CATEGORIES
-- =============================================
SET IDENTITY_INSERT [dbo].[MenuCategories] ON
GO

INSERT INTO [dbo].[MenuCategories] (Id, Name)
VALUES 
(1, N'Khai vị'),
(2, N'Món chính'),
(3, N'Đồ uống'),
(4, N'Tráng miệng')

SET IDENTITY_INSERT [dbo].[MenuCategories] OFF
GO

-- =============================================
-- 3. INSERT RESTAURANTS
-- =============================================
SET IDENTITY_INSERT [dbo].[Restaurants] ON
GO

INSERT INTO [dbo].[Restaurants] (Id, Name, Description, Address, Phone, SellerId, CreatedAt, IsActive, TotalRevenue, AdminCommission)
VALUES 
(1, N'Buffet Poseidon', N'Hải sản & Buffet Quốc tế', N'489 Trường Định', '0912345678', 101, GETDATE(), 1, 0, 0),
(2, N'Lẩu 99 Hoàng Mai', N'Lẩu nướng tự chọn', N'56 Trường Định, Hoàng Mai, Hà Nội', '0988776655', 102, GETDATE(), 1, 0, 0),
(3, N'Quán Cá Lã Vọng', N'Đặc sản Chả Cá Lã Vọng', N'258 Giải Phóng', '0933221100', 103, GETDATE(), 1, 0, 0),
(4, N'Nhà Hàng Hải Sản Nam Biển', N'Hải sản tươi sống', N'25 Định Công, Hoàng Mai, Hà Nội', '0909988776', 104, GETDATE(), 1, 0, 0),
(5, N'Nhà hàng Phở Xưa', N'Ẩm thực Việt Nam', N'102 Giải Phóng, Hoàng Mai, Hà Nội', '0977554433', 105, GETDATE(), 1, 0, 0),
(6, N'Quán Ốc Sài Gòn', N'Ốc & Hải sản bình dân', N'3C Hoàng Mai, Hà Nội', '0966112200', 106, GETDATE(), 1, 0, 0)

SET IDENTITY_INSERT [dbo].[Restaurants] OFF
GO

-- =============================================
-- 4. INSERT MENU ITEMS (50 món ăn)
-- =============================================
SET IDENTITY_INSERT [dbo].[MenuItems] ON
GO

INSERT INTO [dbo].[MenuItems] (Id, Name, Description, Price, RestaurantId, CategoryId, Image, IsAvailable, CreatedAt)
VALUES 
-- KHAI VỊ (Category 1)
(1, N'Càng cua Bạch hoa', N'Càng cua nổi tiếng, chấm với dầm vị.', 59000, 1, 1, 'Khai_vi/cang_cua_bach_hoa.jpg', 1, GETDATE()),
(2, N'Cá trứng lẫn cơm', N'Cá trứng chiên giòn thơm béo.', 59000, 2, 1, 'Khai_vi/ca_trung_lan_com.jpg', 1, GETDATE()),
(3, N'Chả giò', N'Cuốn chiên vàng giòn, nhân thịt rau hấp dẫn.', 35000, 4, 1, 'Khai_vi/cha_gio.jpg', 1, GETDATE()),
(4, N'Chả mực', N'Mực xay nấu đất, thơm vị biển.', 79000, 5, 1, 'Khai_vi/cha_muc.jpg', 1, GETDATE()),
(5, N'Khoai tây chiên', N'Khoai làng giòn, ăn kèm sốt.', 29000, 3, 1, 'Khai_vi/khoai_tay_chien.jpg', 1, GETDATE()),
(6, N'Mực bò tơi', N'Mực xào bơ tỏi thơm béo.', 69000, 6, 1, 'Khai_vi/muc_bo_toi.jpg', 1, GETDATE()),
(7, N'Nem hoa chuối', N'Nem chua rụng tươi thơm mát.', 49000, 3, 1, 'Khai_vi/nom_hoa_chuoi.jpg', 1, GETDATE()),
(8, N'Ốc bưu nhồi thịt', N'Ốc nhồi thịt hươm cay đậm đà.', 79000, 4, 1, 'Khai_vi/oc_buu_nhoi_thit.jpg', 1, GETDATE()),
(9, N'Salad rau củ', N'Rau củ tươi giòn trộn sốt nhẹ.', 45000, 5, 1, 'Khai_vi/salad_raucu.jpg', 1, GETDATE()),
(10, N'Salad rong biển', N'Rong biển thanh mát, nhiều vi chất.', 55000, 2, 1, 'Khai_vi/salad_rongbien.jpg', 1, GETDATE()),
(11, N'Súp bào ngư', N'Súp đậm vị, thêm bào ngư mềm.', 95000, 1, 1, 'Khai_vi/sup_baongu.jpg', 1, GETDATE()),
(12, N'Súp bí đỏ', N'Súp bí đỏ ngọt béo, mịn màng.', 39000, 2, 1, 'Khai_vi/sup_bido.jpg', 1, GETDATE()),
(13, N'Súp cua', N'Súp cua thanh, thịt cua tươi.', 45000, 5, 1, 'Khai_vi/sup_cua.jpg', 1, GETDATE()),
(14, N'Súp gà nọc', N'Súp gà nọc xào mộc mèm tươm.', 39000, 6, 1, 'Khai_vi/sup_gamoc.jpg', 1, GETDATE()),
(15, N'Súp hải sản', N'Súp rau củ với hải sản ngọt vị.', 55000, 6, 1, 'Khai_vi/sup_haisan.jpg', 1, GETDATE()),
(16, N'Tôm chiên tempura', N'Tôm chiên giòn phượng cách Nhật.', 99000, 3, 1, 'Khai_vi/tom_chien_tempura.jpg', 1, GETDATE()),

-- MÓN CHÍNH (Category 2)
(17, N'Bò lá lốt', N'Bò lá lốt nướng thơm.', 69000, 5, 2, 'Mon_chinh/bo_la_lot.jpg', 1, GETDATE()),
(18, N'Bò lúc lắc', N'Bò xốt tiêu xanh mềm thơm.', 109000, 4, 2, 'Mon_chinh/bo_luc_lac.jpg', 1, GETDATE()),
(19, N'Bò sốt vang', N'Thịt bò hầm vang đỏ đậm vị.', 129000, 1, 2, 'Mon_chinh/bo_sot_vang.jpg', 1, GETDATE()),
(20, N'Cá chiên xù', N'Cá phi lê chiên giòn rum.', 79000, 6, 2, 'Mon_chinh/ca_chien_xu.jpg', 1, GETDATE()),
(21, N'Chả cá Lã Vọng', N'Cá nướng vàng đặc trưng Hà Nội.', 129000, 2, 2, 'Mon_chinh/cha_ca_la_vong.jpg', 1, GETDATE()),
(22, N'Cơm chiên dương châu', N'Cơm chiên tôm xúc xích.', 59000, 3, 2, 'Mon_chinh/com_duong_chau.jpg', 1, GETDATE()),
(23, N'Cơm hải sản', N'Cơm chiên hải sản thơm ngon.', 79000, 4, 2, 'Mon_chinh/com_hai_san.jpg', 1, GETDATE()),
(24, N'Cua sốt trứng muối', N'Cua sốt trứng muối béo mặn.', 139000, 6, 2, 'Mon_chinh/cua_sot_trung_muoi.jpg', 1, GETDATE()),
(25, N'Dê xào xả ớt', N'Dê xào cay giòn mềm xả.', 69000, 5, 2, 'Mon_chinh/de_xao_xaot.jpg', 1, GETDATE()),
(26, N'Ếch rang muối', N'Ếch chiên rang muối giòn đậm vị.', 69000, 4, 2, 'Mon_chinh/ech_rang_muoi.jpg', 1, GETDATE()),
(27, N'Gà bơ xối', N'Gà bơ ớt chiên vàng hấp dẫn.', 139000, 3, 2, 'Mon_chinh/ga_bo_xoi.jpg', 1, GETDATE()),
(28, N'Gà chiên mắm', N'Gà chiên giòn sốt mắm đậm đà.', 79000, 2, 2, 'Mon_chinh/ga_chien_mam.jpg', 1, GETDATE()),
(29, N'Mực xào chua ngọt', N'Mực xào giòn ngọt mềm dẻo.', 69000, 1, 2, 'Mon_chinh/muc_xao_chua_ngot.jpg', 1, GETDATE()),
(30, N'Mỳ xào bò', N'Mì xào bò mềm thơm.', 69000, 5, 2, 'Mon_chinh/my_xao_bo.jpg', 1, GETDATE()),
(31, N'Mỳ xào hải sản', N'Mì xào hải sản tươi đậm vị.', 79000, 6, 2, 'Mon_chinh/my_xao_haisan.jpg', 1, GETDATE()),
(32, N'Tôm chiên xù', N'Tôm chiên vàng giòn.', 69000, 3, 2, 'Mon_chinh/tom_chien_xu.jpg', 1, GETDATE()),
(33, N'Vịt quay', N'Vịt quay da giòn, sốt đặc trưng.', 129000, 4, 2, 'Mon_chinh/vit_quay.jpg', 1, GETDATE()),

-- ĐỒ UỐNG (Category 3)
(34, N'7 Up', N'Nước ngọt có gas mát lạnh.', 15000, 6, 3, 'Nuoc_uong/7up.jpg', 1, GETDATE()),
(35, N'Bia 333', N'Bia nhẹ, uống tươi mát.', 20000, 3, 3, 'Nuoc_uong/bia_333.jpg', 1, GETDATE()),
(36, N'Bia Budweiser', N'Bia Mỹ vị nhẹ dễ uống.', 25000, 5, 3, 'Nuoc_uong/bia_bud.jpg', 1, GETDATE()),
(37, N'Bia Sài Gòn', N'Bia quen thuộc đậm vị.', 20000, 5, 3, 'Nuoc_uong/bia_saigon.jpg', 1, GETDATE()),
(38, N'Bia Tiger', N'Bia mạnh, thơm men đặc trưng.', 22000, 2, 3, 'Nuoc_uong/bia_tiger.jpg', 1, GETDATE()),
(39, N'Coca Cola', N'Nước ngọt Coca mát lạnh.', 15000, 1, 3, 'Nuoc_uong/coca.jpg', 1, GETDATE()),
(40, N'Nước lọc', N'Nước tinh khiết dùng chất.', 10000, 2, 3, 'Nuoc_uong/lavie.jpg', 1, GETDATE()),
(41, N'Nước cam', N'Nước cam tự tươi.', 25000, 3, 3, 'Nuoc_uong/nuoc_cam.jpg', 1, GETDATE()),
(42, N'Pepsi', N'Nước ngọt Pepsi vị cola.', 15000, 5, 3, 'Nuoc_uong/pepsi.jpg', 1, GETDATE()),
(43, N'Trà xanh', N'Trà xanh mát lạnh thơm ngon.', 20000, 4, 3, 'Nuoc_uong/tra_xanh.jpg', 1, GETDATE()),

-- TRÁNG MIỆNG (Category 4)
(44, N'Chè bưởi', N'Chè bưởi ngọt mát thanh.', 25000, 2, 4, 'Trang_mieng/che_buoi.jpg', 1, GETDATE()),
(45, N'Chè khúc bạch', N'Chè khúc bạch truyền thống.', 25000, 3, 4, 'Trang_mieng/che_khuc_bach.jpg', 1, GETDATE()),
(46, N'Chè Thái', N'Chè Thái nhiều trái cây.', 30000, 4, 4, 'Trang_mieng/che_thai.jpg', 1, GETDATE()),
(47, N'Chè thập cẩm', N'Chè thập cẩm đầy đủ.', 28000, 1, 4, 'Trang_mieng/che_thap_cam.jpg', 1, GETDATE()),
(48, N'Chè trái cây', N'Chè trái cây tươi mát.', 30000, 5, 4, 'Trang_mieng/che_trai_cay.jpg', 1, GETDATE()),
(49, N'Bánh crepe', N'Crepe mềm thơm nhân ngọt.', 35000, 6, 4, 'Trang_mieng/crep.jpg', 1, GETDATE()),
(50, N'Bánh flan', N'Flan trứng caramel béo mịn.', 19000, 4, 4, 'Trang_mieng/flan.jpg', 1, GETDATE()),
(51, N'Macaron', N'Bánh macaron ngọt nhẹ nhàng màu.', 35000, 3, 4, 'Trang_mieng/macaron.jpg', 1, GETDATE()),
(52, N'Bánh mochi', N'Mochi dẻo nhân kem lạnh.', 29000, 2, 4, 'Trang_mieng/mochi.jpg', 1, GETDATE()),
(53, N'Rau câu trái cây', N'Thạch rau câu trái cây tươi mát.', 25000, 1, 4, 'Trang_mieng/rau_cau_trai_cay.jpg', 1, GETDATE()),
(54, N'Sữa chua nếp cẩm', N'Sữa chua ăn kèm nếp cẩm thơm bùi.', 25000, 3, 4, 'Trang_mieng/sau_chua_nep_cam.jpg', 1, GETDATE()),
(55, N'Sữa chua trái cây', N'Sữa chua trộn trái cây tươi.', 25000, 4, 4, 'Trang_mieng/sua_chua_trai_cay.jpg', 1, GETDATE()),
(56, N'Tiramisu', N'Bánh tiramisu mềm thơm vị cà phê.', 45000, 5, 4, 'Trang_mieng/tiramisu.jpg', 1, GETDATE())

SET IDENTITY_INSERT [dbo].[MenuItems] OFF
GO

PRINT 'Đã insert thành công:'
PRINT '- 6 Customers (Sellers)'
PRINT '- 4 MenuCategories'
PRINT '- 6 Restaurants'
PRINT '- 56 MenuItems (16 Khai vị, 17 Món chính, 10 Đồ uống, 13 Tráng miệng)'
GO
