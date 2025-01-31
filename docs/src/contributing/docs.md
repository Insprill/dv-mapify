# Building Documentation

---

To build Mapify's docs, you'll need to have Python 3 and Pip installed.

- [How to install Python](https://wiki.python.org/moin/BeginnersGuide/Download)
- [How to install pip](https://pip.pypa.io/en/stable/installation/)

To install the required dependencies, you can run the following command:
```shell
python -m pip install -r docs/requirements.txt
```
To serve the documentation locally, you can run
```shell
python -m mkdocs serve
```

It can then be accessed at [http://127.0.0.1:8000/](http://127.0.0.1:8000/).

For information on how MkDocs works, check out [their documentation](https://www.mkdocs.org/getting-started/#adding-pages).
