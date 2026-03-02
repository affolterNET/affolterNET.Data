CREATE SCHEMA IF NOT EXISTS example_pg_history;

CREATE TABLE example_pg_history.demo_table_type(
    id uuid NOT NULL PRIMARY KEY,
    name text NOT NULL
);

CREATE TABLE example_pg_history.demo_table(
    id uuid NOT NULL PRIMARY KEY,
    message text NOT NULL,
    type_id uuid REFERENCES example_pg_history.demo_table_type(id),
    status varchar(50) NOT NULL DEFAULT 'auto default'
);

INSERT INTO example_pg_history.demo_table_type (id, name) VALUES ('c1060bb2-07b0-4e5d-ad0b-35f3993d823d', 'Eins');
INSERT INTO example_pg_history.demo_table_type (id, name) VALUES ('d749abff-6a43-4348-839f-61323fdc52d1', 'Zwei');
INSERT INTO example_pg_history.demo_table_type (id, name) VALUES ('36a072b9-7216-4b99-bf8d-79730a4a1f37', 'Drei');
INSERT INTO example_pg_history.demo_table_type (id, name) VALUES ('230a5728-acb6-4e91-aea3-05ef34c0755d', 'Vier');
INSERT INTO example_pg_history.demo_table (id, message, type_id) VALUES (gen_random_uuid(), 'It is working!', '36a072b9-7216-4b99-bf8d-79730a4a1f37');

CREATE VIEW example_pg_history.v_demo
AS SELECT * FROM example_pg_history.demo_table;
