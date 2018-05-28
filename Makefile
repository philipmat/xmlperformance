.PHONY: docs

LABELS = labels-20180514_test56.xml
ARTISTS = artists-20180514_test50.xml

# tests:
# 	pipenv run python -m unittest
# init:
# 	python setup.py install
# 	pipenv install --dev --three

net-labels:
	dotnet run --project dotnet-core/xmlperformance.csproj ./data/$(LABELS)
net-artists:
	dotnet run --project dotnet-core/xmlperformance.csproj ./data/$(ARTISTS)
net-all: net-labels net-artists

py-labels:
	python3 python/main.py ./data/$(LABELS)

py-artists:
	python3 python/main.py ./data/$(LABELS)

py-all: py-labels py-artists