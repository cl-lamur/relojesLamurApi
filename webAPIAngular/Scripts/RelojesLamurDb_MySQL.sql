-- ============================================================
--  RELOJES LAMUR — Script de creación de base de datos MySQL
--  Generado a partir del modelo EF Core del proyecto
--  Compatible con: MySQL 8.0+
-- ============================================================

DROP DATABASE IF EXISTS RelojesLamurDb;
CREATE DATABASE RelojesLamurDb
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE RelojesLamurDb;

-- ????????????????????????????????????????????????????????????
--  TABLAS
-- ????????????????????????????????????????????????????????????

-- ?? Users ???????????????????????????????????????????????????
CREATE TABLE Users (
    Id           CHAR(36)     NOT NULL,
    Name         VARCHAR(100) NOT NULL,
    Email        VARCHAR(200) NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    Role         VARCHAR(20)  NOT NULL DEFAULT 'user',
    IsDeleted    TINYINT(1)   NOT NULL DEFAULT 0,
    CreatedAt    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),

    -- Índice para búsquedas por email (login)
    INDEX IX_Users_Email (Email)
) ENGINE=InnoDB;

-- ?? Products ????????????????????????????????????????????????
CREATE TABLE Products (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Name        VARCHAR(150) NOT NULL,
    Description VARCHAR(1000) NOT NULL,
    Price       DECIMAL(10,2) NOT NULL,
    Category    VARCHAR(10)  NOT NULL COMMENT '"hombre" | "mujer"',
    ImageUrl    VARCHAR(500) NOT NULL,
    InStock     TINYINT(1)   NOT NULL DEFAULT 1,
    IsDeleted   TINYINT(1)   NOT NULL DEFAULT 0,
    CreatedAt   DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt   DATETIME     NULL     DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT PK_Products PRIMARY KEY (Id),

    INDEX IX_Products_Category (Category),
    INDEX IX_Products_Price (Price),
    INDEX IX_Products_IsDeleted (IsDeleted),
    FULLTEXT INDEX FT_Products_Search (Name, Description)
) ENGINE=InnoDB;

-- ?? ProductFeatures (1:1 con Products) ?????????????????????
CREATE TABLE ProductFeatures (
    Id              INT          NOT NULL AUTO_INCREMENT,
    ProductId       INT          NOT NULL,
    Material        VARCHAR(200) NULL,
    Mechanism       VARCHAR(200) NULL,
    WaterResistance VARCHAR(100) NULL,

    CONSTRAINT PK_ProductFeatures PRIMARY KEY (Id),
    CONSTRAINT UQ_ProductFeatures_ProductId UNIQUE (ProductId),
    CONSTRAINT FK_ProductFeatures_Products
        FOREIGN KEY (ProductId) REFERENCES Products(Id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ?? Orders ??????????????????????????????????????????????????
CREATE TABLE Orders (
    Id        CHAR(36)      NOT NULL,
    UserId    CHAR(36)      NOT NULL,
    Subtotal  DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    Tax       DECIMAL(10,2) NOT NULL DEFAULT 0.00 COMMENT 'IVA 21%',
    Total     DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    Status    VARCHAR(20)   NOT NULL DEFAULT 'pending' COMMENT '"pending" | "confirmed" | "cancelled"',
    CreatedAt DATETIME      NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT PK_Orders PRIMARY KEY (Id),
    CONSTRAINT FK_Orders_Users
        FOREIGN KEY (UserId) REFERENCES Users(Id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    INDEX IX_Orders_UserId (UserId),
    INDEX IX_Orders_Status (Status),
    INDEX IX_Orders_CreatedAt (CreatedAt DESC)
) ENGINE=InnoDB;

-- ?? OrderItems ??????????????????????????????????????????????
CREATE TABLE OrderItems (
    Id        INT           NOT NULL AUTO_INCREMENT,
    OrderId   CHAR(36)      NOT NULL,
    ProductId INT           NOT NULL,
    Quantity  INT           NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL COMMENT 'Precio congelado al momento de la compra',

    CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
    CONSTRAINT FK_OrderItems_Orders
        FOREIGN KEY (OrderId) REFERENCES Orders(Id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_OrderItems_Products
        FOREIGN KEY (ProductId) REFERENCES Products(Id)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,

    CONSTRAINT CHK_OrderItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT CHK_OrderItems_UnitPrice CHECK (UnitPrice >= 0),

    INDEX IX_OrderItems_OrderId (OrderId),
    INDEX IX_OrderItems_ProductId (ProductId)
) ENGINE=InnoDB;

-- ????????????????????????????????????????????????????????????
--  SEED DATA
-- ????????????????????????????????????????????????????????????

-- ?? Admin user ??????????????????????????????????????????????
-- Password: Admin123!  (BCrypt cost 12)
INSERT INTO Users (Id, Name, Email, PasswordHash, Role, IsDeleted, CreatedAt)
VALUES (
    UUID(),
    'Administrador Lamur',
    'admin@lamur.com',
    '$2a$12$LJ3m4ys3Gz8y8Qhg0bSOp.VcGF/VKDEql4fVMr2BHkqMLbHPGfHzW',
    'admin',
    0,
    UTC_TIMESTAMP()
);

-- ?? Productos: Hombre (6) ??????????????????????????????????
INSERT INTO Products (Name, Description, Price, Category, ImageUrl, InStock, IsDeleted, CreatedAt) VALUES
('Lamur Explorer I',      'Reloj de exploración con bisel negro anodizado y resistencia extrema para aventureros.',              349.99, 'hombre', '/images/explorer-i.jpg',      1, 0, UTC_TIMESTAMP()),
('Lamur Mariner Pro',     'Diseñado para buceadores profesionales con correa de caucho vulcanizado y cerámica.',                 449.99, 'hombre', '/images/mariner-pro.jpg',     1, 0, UTC_TIMESTAMP()),
('Lamur Racing Heritage', 'Inspirado en la alta velocidad. Cronógrafo con taquímetro y pulsómetro integrados.',                  289.99, 'hombre', '/images/racing-heritage.jpg',  1, 0, UTC_TIMESTAMP()),
('Lamur Aviator GMT',     'Doble zona horaria para el viajero moderno. Bisel giratorio bidireccional.',                          499.99, 'hombre', '/images/aviator-gmt.jpg',     1, 0, UTC_TIMESTAMP()),
('Lamur Pilot Classic',   'Estética militar y precisión suiza. Esfera negro mate con marcadores Super-LumiNova.',                429.99, 'hombre', '/images/pilot-classic.jpg',    1, 0, UTC_TIMESTAMP()),
('Lamur Diver 300',       'Reloj de buceo sport con válvula de helio y cerrojo de seguridad en la correa.',                      249.99, 'hombre', '/images/diver-300.jpg',       1, 0, UTC_TIMESTAMP());

-- ?? Productos: Mujer (6) ???????????????????????????????????
INSERT INTO Products (Name, Description, Price, Category, ImageUrl, InStock, IsDeleted, CreatedAt) VALUES
('Lamur Rose Datejust',       'Elegancia atemporal con esfera nacarada y bisel engastado de cristales.',                         549.99, 'mujer', '/images/rose-datejust.jpg',       1, 0, UTC_TIMESTAMP()),
('Lamur Constellation Lady',  'Forma estrella característica en bisel, brazalete integrado y esfera de brillantes.',             389.99, 'mujer', '/images/constellation-lady.jpg',  1, 0, UTC_TIMESTAMP()),
('Lamur Diamond Tank',        'Silueta rectangular clásica con incrustaciones de cristales Swarovski en caja.',                  479.99, 'mujer', '/images/diamond-tank.jpg',        1, 0, UTC_TIMESTAMP()),
('Lamur Pearl Collection',    'Correa de piel italiana genuina y esfera con motivo floral sobre madreperla.',                    399.99, 'mujer', '/images/pearl-collection.jpg',    1, 0, UTC_TIMESTAMP()),
('Lamur Elegance Slim',       'Perfil ultra delgado de 5 mm. Minimalismo francés en su máxima expresión.',                       229.99, 'mujer', '/images/elegance-slim.jpg',       1, 0, UTC_TIMESTAMP()),
('Lamur Petite Perle',        'Diseño compacto y femenino con corona de zafiro sintético. Ideal para uso diario.',               129.99, 'mujer', '/images/petite-perle.jpg',        1, 0, UTC_TIMESTAMP());

-- ?? ProductFeatures (vinculados por Id secuencial) ??????????
INSERT INTO ProductFeatures (ProductId, Material, Mechanism, WaterResistance) VALUES
(1,  'Acero inoxidable 316L',              'Automático ETA 2824-2',          '300m'),
(2,  'Cerámica y titanio grado 5',         'Automático Sellita SW200',       '500m'),
(3,  'Acero inoxidable y fibra de carbono', 'Cuarzo cronógrafo Swiss ISA 8171','100m'),
(4,  'Acero inoxidable PVD negro',          'Automático NH34A GMT',           '200m'),
(5,  'Acero inoxidable cepillado',          'Automático ETA 2892-A2',         '100m'),
(6,  'Acero inoxidable 904L',              'Automático Miyota 9015',          '300m'),
(7,  'Oro rosa 18K y acero',               'Automático Cal. 3235',            '100m'),
(8,  'Acero inoxidable y oro amarillo',     'Cuarzo Swiss ETA F04.111',        '30m'),
(9,  'Acero PVD dorado',                   'Cuarzo Swiss Ronda 762',          '30m'),
(10, 'Acero inoxidable y madreperla',       'Cuarzo Miyota 1L45',             '50m'),
(11, 'Acero inoxidable cepillado',          'Cuarzo Swiss ISA 1198/250',       '30m'),
(12, 'Acero inoxidable 316L',              'Cuarzo Ronda 784',                '50m');

-- ????????????????????????????????????????????????????????????
--  VISTAS ÚTILES
-- ????????????????????????????????????????????????????????????

-- Vista: Productos activos con sus características
CREATE OR REPLACE VIEW vw_ProductosActivos AS
SELECT
    p.Id,
    p.Name,
    p.Description,
    p.Price,
    p.Category,
    p.ImageUrl,
    p.InStock,
    p.CreatedAt,
    p.UpdatedAt,
    pf.Material,
    pf.Mechanism,
    pf.WaterResistance
FROM Products p
LEFT JOIN ProductFeatures pf ON pf.ProductId = p.Id
WHERE p.IsDeleted = 0;

-- Vista: Pedidos con info de usuario
CREATE OR REPLACE VIEW vw_PedidosConUsuario AS
SELECT
    o.Id        AS OrderId,
    o.Subtotal,
    o.Tax,
    o.Total,
    o.Status,
    o.CreatedAt AS OrderDate,
    u.Id        AS UserId,
    u.Name      AS UserName,
    u.Email     AS UserEmail
FROM Orders o
INNER JOIN Users u ON u.Id = o.UserId
WHERE u.IsDeleted = 0;

-- Vista: Detalle completo de items de pedido
CREATE OR REPLACE VIEW vw_DetalleOrderItems AS
SELECT
    oi.Id        AS ItemId,
    oi.OrderId,
    oi.Quantity,
    oi.UnitPrice,
    (oi.Quantity * oi.UnitPrice) AS LineTotal,
    p.Id         AS ProductId,
    p.Name       AS ProductName,
    p.ImageUrl   AS ProductImageUrl,
    p.Category
FROM OrderItems oi
INNER JOIN Products p ON p.Id = oi.ProductId;

-- ????????????????????????????????????????????????????????????
--  STORED PROCEDURES ÚTILES
-- ????????????????????????????????????????????????????????????

-- SP: Obtener productos con filtros y paginación
DELIMITER //
CREATE PROCEDURE sp_GetProducts(
    IN p_Category   VARCHAR(10),
    IN p_MinPrice   DECIMAL(10,2),
    IN p_MaxPrice   DECIMAL(10,2),
    IN p_Search     VARCHAR(200),
    IN p_Page       INT,
    IN p_PageSize   INT
)
BEGIN
    DECLARE v_Offset INT;

    SET p_Page     = IFNULL(p_Page, 1);
    SET p_PageSize = LEAST(IFNULL(p_PageSize, 12), 50);
    SET v_Offset   = (p_Page - 1) * p_PageSize;

    -- Total para paginación
    SELECT COUNT(*) AS Total
    FROM Products
    WHERE IsDeleted = 0
      AND (p_Category IS NULL OR Category = p_Category)
      AND (p_MinPrice IS NULL OR Price >= p_MinPrice)
      AND (p_MaxPrice IS NULL OR Price <= p_MaxPrice)
      AND (p_Search   IS NULL OR Name LIKE CONCAT('%', p_Search, '%')
                               OR Description LIKE CONCAT('%', p_Search, '%'));

    -- Datos paginados
    SELECT *
    FROM Products
    WHERE IsDeleted = 0
      AND (p_Category IS NULL OR Category = p_Category)
      AND (p_MinPrice IS NULL OR Price >= p_MinPrice)
      AND (p_MaxPrice IS NULL OR Price <= p_MaxPrice)
      AND (p_Search   IS NULL OR Name LIKE CONCAT('%', p_Search, '%')
                               OR Description LIKE CONCAT('%', p_Search, '%'))
    ORDER BY CreatedAt DESC
    LIMIT p_PageSize OFFSET v_Offset;
END //
DELIMITER ;

-- SP: Crear pedido completo (cabecera)
DELIMITER //
CREATE PROCEDURE sp_CreateOrder(
    IN  p_UserId   CHAR(36),
    IN  p_Subtotal DECIMAL(10,2),
    OUT p_OrderId  CHAR(36)
)
BEGIN
    DECLARE v_Tax   DECIMAL(10,2);
    DECLARE v_Total DECIMAL(10,2);

    SET p_OrderId = UUID();
    SET v_Tax     = ROUND(p_Subtotal * 0.21, 2);
    SET v_Total   = p_Subtotal + v_Tax;

    INSERT INTO Orders (Id, UserId, Subtotal, Tax, Total, Status, CreatedAt)
    VALUES (p_OrderId, p_UserId, p_Subtotal, v_Tax, v_Total, 'pending', UTC_TIMESTAMP());
END //
DELIMITER ;

-- ????????????????????????????????????????????????????????????
--  VERIFICACIÓN
-- ????????????????????????????????????????????????????????????

SELECT '?? Tablas creadas ??' AS Info;
SELECT TABLE_NAME, ENGINE, TABLE_ROWS
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'RelojesLamurDb'
  AND TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

SELECT '?? Datos seed ??' AS Info;
SELECT 'Users'    AS Tabla, COUNT(*) AS Registros FROM Users
UNION ALL
SELECT 'Products',         COUNT(*) FROM Products
UNION ALL
SELECT 'ProductFeatures',  COUNT(*) FROM ProductFeatures;

SELECT '? Base de datos RelojesLamurDb creada correctamente.' AS Resultado;
