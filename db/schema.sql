-- Учебная практика УП.01.01. Тема №63: система управления сетью пиццерий
-- СУБД: Microsoft SQL Server
IF DB_ID(N'PizzeriaChainDB') IS NULL
    CREATE DATABASE PizzeriaChainDB;
GO
USE PizzeriaChainDB;
GO

-- Пиццерии (аналог Category)
CREATE TABLE dbo.pizzeria (
    id      INT IDENTITY(1,1) PRIMARY KEY,
    name    NVARCHAR(100) NOT NULL,
    address NVARCHAR(200) NOT NULL,
    city    NVARCHAR(100) NOT NULL,
    phone   NVARCHAR(20)  NOT NULL
);
GO

-- Клиенты и учётные записи (аналог Users)
CREATE TABLE dbo.customer (
    id            INT IDENTITY(1,1) PRIMARY KEY,
    login         NVARCHAR(50)  NOT NULL UNIQUE,
    password_hash CHAR(64)      NOT NULL,   -- SHA-256, hex
    salt          CHAR(32)      NOT NULL,   -- hex
    role          NVARCHAR(20)  NOT NULL
                  CONSTRAINT CK_customer_role CHECK (role IN (N'Admin', N'User')),
    full_name     NVARCHAR(100) NOT NULL,
    phone         NVARCHAR(20)  NULL,
    email         NVARCHAR(100) NULL,
    bonus_points  INT           NOT NULL
                  CONSTRAINT DF_customer_bonus DEFAULT 0
                  CONSTRAINT CK_customer_bonus CHECK (bonus_points >= 0)
);
GO

-- Заказы (аналог Payment)
CREATE TABLE dbo.customer_order (
    id           INT IDENTITY(1,1) PRIMARY KEY,
    customer_id  INT NOT NULL,
    pizzeria_id  INT NOT NULL,
    order_date   DATETIME NOT NULL CONSTRAINT DF_order_date DEFAULT GETDATE(),
    order_type   NVARCHAR(20) NOT NULL
                 CONSTRAINT CK_order_type CHECK (order_type IN (N'В зале', N'Самовывоз', N'Доставка')),
    status       NVARCHAR(20) NOT NULL
                 CONSTRAINT CK_order_status CHECK (status IN (N'Новый', N'Готовится', N'Выдан', N'Отменён')),
    total_amount DECIMAL(10,2) NOT NULL CONSTRAINT CK_order_total CHECK (total_amount > 0),
    CONSTRAINT FK_order_customer FOREIGN KEY (customer_id)
        REFERENCES dbo.customer(id) ON DELETE CASCADE,
    CONSTRAINT FK_order_pizzeria FOREIGN KEY (pizzeria_id)
        REFERENCES dbo.pizzeria(id) ON DELETE CASCADE
);
GO

-- Тестовые данные
INSERT INTO dbo.pizzeria (name, address, city, phone) VALUES
(N'Пицца Классика', N'ул. Ленина, 10',   N'Москва',  N'+74951110001'),
(N'Пицца Классика', N'пр. Мира, 25',     N'Москва',  N'+74951110002'),
(N'Пицца Классика', N'ул. Горького, 5',  N'Химки',   N'+74951110003'),
(N'Пицца Классика', N'ул. Победы, 12',   N'Балашиха',N'+74951110004');

-- Пароли-заглушки: реальные хэши приложение создаёт при регистрации.
-- Для входа админом создайте его через форму регистрации и вручную смените role на Admin.
INSERT INTO dbo.customer (login, password_hash, salt, role, full_name, phone, email, bonus_points) VALUES
(N'admin',  REPLICATE('0',64), REPLICATE('0',32), N'Admin', N'Администратор Системы', N'+79990000001', N'admin@pizza.local', 0),
(N'ivanov', REPLICATE('0',64), REPLICATE('0',32), N'User',  N'Иванов Пётр Сергеевич', N'+79990000002', N'ivanov@mail.ru', 120),
(N'petrova',REPLICATE('0',64), REPLICATE('0',32), N'User',  N'Петрова Анна Игоревна', N'+79990000003', N'petrova@mail.ru', 45),
(N'sidorov',REPLICATE('0',64), REPLICATE('0',32), N'User',  N'Сидоров Алексей Павлович', N'+79990000004', NULL, 0);

INSERT INTO dbo.customer_order (customer_id, pizzeria_id, order_date, order_type, status, total_amount) VALUES
(2, 1, '2026-09-01T12:30', N'В зале',    N'Выдан',     1250.00),
(2, 2, '2026-09-03T19:10', N'Доставка',  N'Выдан',     1890.50),
(3, 1, '2026-09-05T14:00', N'Самовывоз', N'Выдан',      760.00),
(3, 3, '2026-09-07T20:45', N'Доставка',  N'Готовится', 2310.00),
(4, 4, '2026-09-10T13:15', N'В зале',    N'Новый',      980.00),
(4, 2, '2026-09-12T18:20', N'Самовывоз', N'Отменён',    640.00);
GO