-- =============================================================================
-- SISTEMA DE GESTION PARA LA COOPERATIVA FINANCIERA EL PROGRESO
-- Script de Creacion de Base de Datos y Esquema Relacional (DDL)
-- Compatible con: PostgreSQL / MySQL 8+ / SQL Server (ANSI SQL estandar)
-- =============================================================================

-- 1. TABLA: ROLES DE USUARIO (Cajero, Gerente)
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255)
);

-- 2. TABLA: USUARIOS DEL SISTEMA
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    role_id INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_roles FOREIGN KEY (role_id) REFERENCES roles(id)
);

-- 3. TABLA: ASOCIADOS (CLIENTES DE LA COOPERATIVA)
-- Regla 1: Saldo inicial $0 COP (calculado con sus movimientos)
-- Regla 2: Documento unico (sin duplicados)
CREATE TABLE IF NOT EXISTS associates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_number VARCHAR(20) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    phone VARCHAR(25) NOT NULL,
    address VARCHAR(200) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 4. TABLA: TIPOS DE TRANSACCION (Deposito, Retiro)
CREATE TABLE IF NOT EXISTS transaction_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    code VARCHAR(20) NOT NULL UNIQUE
);

-- 5. TABLA: TRANSACCIONES FINANCIERAS (MOVIMIENTOS)
-- Regla 3: Monto mayor a cero
-- Regla 4: Tarifa de $8.000 COP si retiro > $1.000.000 COP
-- Regla 5: ON DELETE RESTRICT (No se pueden eliminar asociados con movimientos)
CREATE TABLE IF NOT EXISTS transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    associate_id UUID NOT NULL,
    user_id UUID NULL,
    type_id INT NOT NULL,
    amount DECIMAL(18, 2) NOT NULL CHECK (amount > 0),
    commission DECIMAL(18, 2) NOT NULL DEFAULT 0.00 CHECK (commission >= 0),
    date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_transactions_associates 
        FOREIGN KEY (associate_id) 
        REFERENCES associates(id) 
        ON DELETE RESTRICT,
    CONSTRAINT fk_transactions_users 
        FOREIGN KEY (user_id) 
        REFERENCES users(id) 
        ON DELETE SET NULL,
    CONSTRAINT fk_transactions_types 
        FOREIGN KEY (type_id) 
        REFERENCES transaction_types(id)
);

-- =============================================================================
-- INDICES PARA OPTIMIZACION DE BUSQUEDAS Y REPORTES
-- =============================================================================
CREATE INDEX IF NOT EXISTS idx_associates_doc ON associates(document_number);
CREATE INDEX IF NOT EXISTS idx_associates_name ON associates(name);
CREATE INDEX IF NOT EXISTS idx_transactions_associate ON transactions(associate_id);
CREATE INDEX IF NOT EXISTS idx_transactions_date ON transactions(date);
CREATE INDEX IF NOT EXISTS idx_transactions_amount ON transactions(amount DESC);

-- =============================================================================
-- VISTAS ANALITICAS PARA REPORTES GERENCIALES
-- =============================================================================

-- VISTA: Saldo consolidado por cada asociado en tiempo real
CREATE OR REPLACE VIEW vw_associate_balances AS
SELECT 
    a.id AS associate_id,
    a.document_number,
    a.name AS associate_name,
    a.phone,
    a.address,
    a.created_at,
    COALESCE(SUM(
        CASE 
            WHEN tt.code = "DEPOSIT" THEN t.amount
            WHEN tt.code = "WITHDRAWAL" THEN -(t.amount + t.commission)
            ELSE 0 
        END
    ), 0.00) AS current_balance,
    COUNT(t.id) AS total_movements
FROM associates a
LEFT JOIN transactions t ON a.id = t.associate_id
LEFT JOIN transaction_types tt ON t.type_id = tt.id
GROUP BY a.id, a.document_number, a.name, a.phone, a.address, a.created_at;

-- VISTA: Informe 1 - Resumen General de la Cooperativa
CREATE OR REPLACE VIEW vw_cooperative_summary AS
SELECT 
    COUNT(*) AS total_associates,
    COALESCE(SUM(current_balance), 0) AS total_balance,
    COALESCE(AVG(current_balance), 0) AS average_balance
FROM vw_associate_balances;

-- VISTA: Informe 3 - Asociados Inactivos (Saldo 0 y sin transacciones)
CREATE OR REPLACE VIEW vw_inactive_associates AS
SELECT 
    associate_id,
    document_number,
    associate_name,
    phone,
    current_balance,
    total_movements
FROM vw_associate_balances
WHERE current_balance = 0 AND total_movements = 0;

-- =============================================================================
-- FUNCION / PROCEDIMIENTO: Registro de Retiro con Reglas de Negocio
-- =============================================================================
CREATE OR REPLACE FUNCTION fn_register_withdrawal(
    p_associate_id UUID,
    p_amount DECIMAL(18, 2),
    p_user_id UUID DEFAULT NULL
)
RETURNS JSON AS $$
DECLARE
    v_current_balance DECIMAL(18, 2);
    v_commission DECIMAL(18, 2) := 0.00;
    v_total_deduction DECIMAL(18, 2);
    v_withdrawal_type_id INT;
    v_new_tx_id UUID := gen_random_uuid();
    v_new_balance DECIMAL(18, 2);
BEGIN
    IF p_amount <= 0 THEN
        RAISE EXCEPTION "El monto a retirar debe ser mayor a cero.";
    END IF;

    SELECT id INTO v_withdrawal_type_id FROM transaction_types WHERE code = "WITHDRAWAL";
    IF v_withdrawal_type_id IS NULL THEN
        RAISE EXCEPTION "Tipo de transaccion WITHDRAWAL no encontrado.";
    END IF;

    -- Cobro de comision si supera 1.000.000 COP
    IF p_amount > 1000000 THEN
        v_commission := 8000.00;
    END IF;
    v_total_deduction := p_amount + v_commission;

    SELECT current_balance INTO v_current_balance 
    FROM vw_associate_balances 
    WHERE associate_id = p_associate_id;

    IF v_current_balance IS NULL THEN
        RAISE EXCEPTION "Asociado no encontrado.";
    END IF;

    IF v_current_balance < v_total_deduction THEN
        RAISE EXCEPTION "Fondos insuficientes. La operacion dejaria el saldo en negativo.";
    END IF;

    INSERT INTO transactions (id, associate_id, user_id, type_id, amount, commission, date)
    VALUES (v_new_tx_id, p_associate_id, p_user_id, v_withdrawal_type_id, p_amount, v_commission, CURRENT_TIMESTAMP);

    v_new_balance := v_current_balance - v_total_deduction;

    RETURN json_build_object(
        "transaction_id", v_new_tx_id,
        "amount", p_amount,
        "commission", v_commission,
        "new_balance", v_new_balance,
        "status", "SUCCESS"
    );
END;
$$ LANGUAGE plpgsql;
