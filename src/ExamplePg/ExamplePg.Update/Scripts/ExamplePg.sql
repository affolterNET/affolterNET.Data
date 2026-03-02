CREATE SCHEMA IF NOT EXISTS example_pg;

CREATE TABLE example_pg.demo_table_type(
    id uuid NOT NULL PRIMARY KEY,
    name text NOT NULL
);

CREATE TABLE example_pg.demo_table(
    id uuid NOT NULL PRIMARY KEY,
    message text NOT NULL,
    type_id uuid REFERENCES example_pg.demo_table_type(id),
    status varchar(50) NOT NULL DEFAULT 'auto default',
    date_test date NOT NULL,
    date_end_test date
);

INSERT INTO example_pg.demo_table_type (id, name) VALUES ('c1060bb2-07b0-4e5d-ad0b-35f3993d823d', 'Eins');
INSERT INTO example_pg.demo_table_type (id, name) VALUES ('d749abff-6a43-4348-839f-61323fdc52d1', 'Zwei');
INSERT INTO example_pg.demo_table_type (id, name) VALUES ('36a072b9-7216-4b99-bf8d-79730a4a1f37', 'Drei');
INSERT INTO example_pg.demo_table (id, message, type_id, date_test) VALUES (gen_random_uuid(), 'It is working!', '36a072b9-7216-4b99-bf8d-79730a4a1f37', CURRENT_DATE);

CREATE VIEW example_pg.v_demo
AS SELECT * FROM example_pg.demo_table;
