import gzip
import sys
from xml.sax import make_parser
from parser import LabelsHandler


# http://www.discogs.com/help/voting-guidelines.html
DATA_QUALITY_VALUES = (
    'Needs Vote',
    'Complete And Correct',
    'Correct',
    'Needs Minor Changes',
    'Needs Major Changes',
    'Entirely Incorrect',
    'Entirely Incorrect Edit'
)


class StatsCollector:
    def __init__(self):
        global DATA_QUALITY_VALUES
        self._data_quality = {dq: 0 for dq in DATA_QUALITY_VALUES}

    def collect(self, model):
        if model.id == 0:
            return
        quality = model.data_quality
        if quality in self._data_quality:
            self._data_quality[quality] += 1

    def __str__(self):
        # print(self._data_quality)
        totals = sum(self._data_quality.values())
        lines = "\n".join([f"{key.ljust(25)} = {value: >10,}" for key, value in self._data_quality.items()])
        return f"Total: {totals} entries.\n{lines}"


def make_handler(file: str, collector):
    if 'labels' in file:
        return LabelsHandler(collector)
    return None


def parse(file: str, parser) -> None:
    _open = open
    if file.endswith(".gz"):
        _open = gzip.open
    with _open(file) as f:
        parser.parse(f)


def main(argv):
    file = argv[0]
    collector = StatsCollector()
    handler = make_handler(file, collector)
    if handler is None:
        print(f"I don't know how to handle {file}.")
        sys.exit(2)

    parser = make_parser()
    parser.setContentHandler(handler)
    parse(file, parser)
    print(str(collector))


if __name__ == "__main__":
    main(sys.argv[1:])
