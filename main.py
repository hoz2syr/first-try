import sqlite3

def init_db(db_name="people.db"):
    conn = sqlite3.connect(db_name)
    c = conn.cursor()
    c.execute("""CREATE TABLE IF NOT EXISTS people (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        name TEXT NOT NULL,
        age INTEGER NOT NULL
    )""")
    conn.commit()
    return conn

def add_person(conn, name, age):
    c = conn.cursor()
    c.execute("INSERT INTO people (name, age) VALUES (?, ?)", (name, age))
    conn.commit()


def main():
    conn = init_db()
    name = input("Enter your name: ")
    age_input = input("Enter your age: ")
    try:
        age = int(age_input)
    except ValueError:
        print("Age must be a number.")
        return
    add_person(conn, name, age)
    print(f"Saved {name} ({age}) to the database.")

if __name__ == "__main__":
    main()
