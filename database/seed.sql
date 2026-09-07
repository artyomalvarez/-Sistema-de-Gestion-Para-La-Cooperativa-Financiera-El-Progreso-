-- =============================================================================
-- SISTEMA DE GESTION PARA LA COOPERATIVA FINANCIERA EL PROGRESO
-- Script de Insercion de Datos Semilla (Seed Data)
-- =============================================================================

-- 1. Insertar Roles
INSERT INTO roles (id, name, description) VALUES
(1, "Cashier", "Cajero de ventanilla con permisos de operacion financiera"),
(2, "Manager", "Gerente administrativo con acceso a informes analiticos")
ON CONFLICT (id) DO NOTHING;

-- 2. Insertar Tipos de Transaccion
INSERT INTO transaction_types (id, name, code) VALUES
(1, "Deposito", "DEPOSIT"),
(2, "Retiro", "WITHDRAWAL")
ON CONFLICT (id) DO NOTHING;

-- 3. Insertar Usuarios
INSERT INTO users (id, username, name, role_id) VALUES
("a0000000-0000-0000-0000-000000000001", "cajero1", "Carlos Ventanilla", 1),
("a0000000-0000-0000-0000-000000000002", "gerente1", "Laura Administradora", 2)
ON CONFLICT (id) DO NOTHING;

-- 4. Insertar Asociados Iniciales
INSERT INTO associates (id, document_number, name, phone, address, created_at) VALUES
("b0000000-0000-0000-0000-000000000001", "1001", "Carlos Perez", "3001234567", "Calle 10 # 20-30", CURRENT_TIMESTAMP - INTERVAL "30 days"),
("b0000000-0000-0000-0000-000000000002", "1002", "Maria Gomez", "3109876543", "Carrera 15 # 45-12", CURRENT_TIMESTAMP - INTERVAL "20 days"),
("b0000000-0000-0000-0000-000000000003", "1003", "Juan Rodriguez", "3205551122", "Avenida Siempre Viva 742", CURRENT_TIMESTAMP - INTERVAL "10 days"),
("b0000000-0000-0000-0000-000000000004", "1004", "Ana Inactiva", "3150000000", "Diagonal 5 # 1-2", CURRENT_TIMESTAMP - INTERVAL "5 days")
ON CONFLICT (id) DO NOTHING;

-- 5. Insertar Transacciones Iniciales
-- Carlos Perez: Deposito 2.500.000, Retiro 500.000 (Saldo: 2.000.000)
INSERT INTO transactions (id, associate_id, user_id, type_id, amount, commission, date) VALUES
("c0000000-0000-0000-0000-000000000001", "b0000000-0000-0000-0000-000000000001", "a0000000-0000-0000-0000-000000000001", 1, 2500000.00, 0.00, CURRENT_TIMESTAMP - INTERVAL "25 days"),
("c0000000-0000-0000-0000-000000000002", "b0000000-0000-0000-0000-000000000001", "a0000000-0000-0000-0000-000000000001", 2, 500000.00, 0.00, CURRENT_TIMESTAMP - INTERVAL "15 days");

-- Maria Gomez: Deposito 5.000.000, Retiro 1.500.000 con comision 8.000 (Saldo: 3.492.000)
INSERT INTO transactions (id, associate_id, user_id, type_id, amount, commission, date) VALUES
("c0000000-0000-0000-0000-000000000003", "b0000000-0000-0000-0000-000000000002", "a0000000-0000-0000-0000-000000000001", 1, 5000000.00, 0.00, CURRENT_TIMESTAMP - INTERVAL "18 days"),
("c0000000-0000-0000-0000-000000000004", "b0000000-0000-0000-0000-000000000002", "a0000000-0000-0000-0000-000000000001", 2, 1500000.00, 8000.00, CURRENT_TIMESTAMP - INTERVAL "8 days");

-- Juan Rodriguez: Deposito 800.000 (Saldo: 800.000)
INSERT INTO transactions (id, associate_id, user_id, type_id, amount, commission, date) VALUES
("c0000000-0000-0000-0000-000000000005", "b0000000-0000-0000-0000-000000000003", "a0000000-0000-0000-0000-000000000001", 1, 800000.00, 0.00, CURRENT_TIMESTAMP - INTERVAL "2 days");

-- Ana Inactiva: Sin transacciones (Saldo: 0.00)
