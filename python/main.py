import gzip
import sys
from xml.sax import make_parser
from handlers import LabelsHandler
import stats


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
    collector = stats.StatsCollector()
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
