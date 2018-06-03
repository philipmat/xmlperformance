.PHONY: compile

LABELS = labels-20180514_test56.xml
ARTISTS = artists-20180514_test50.xml

run-all: net-all py-all js-all

# .NET

net-compile:
	dotnet build dotnet-compile/xmlperformance.csproj
net-labels:
	dotnet run --project dotnet-core/xmlperformance.csproj ./data/$(LABELS)
net-artists:
	dotnet run --project dotnet-core/xmlperformance.csproj ./data/$(ARTISTS)
net-all: net-labels net-artists

# PYTHON

PYTHON_FILES = python/handlers.py python/main.py python/models.py python/stats.py
.py:
	python3 -m py_compile $<

py-compile:
	echo $(PYTHON_FILES)
	python3 -m py_compile $(PYTHON_FILES)

py-labels:
	python3 python/main.py ./data/$(LABELS)

py-artists:
	python3 python/main.py ./data/$(ARTISTS)

py-all: py-labels py-artists

js-labels:
	node nodejs/index.js ./data/$(LABELS)

js-artists:
	node nodejs/index.js ./data/$(ARTISTS)

js-all: js-labels js-artists

# tests:
# 	pipenv run python -m unittest
# init:
# 	python setup.py install
# 	pipenv install --dev --three
