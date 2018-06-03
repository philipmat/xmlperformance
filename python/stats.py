# http://www.discogs.com/help/voting-guidelines.html
DATA_QUALITY_VALUES = (
    'Needs Vote',
    'Complete and Correct',
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
        self._keymap = {dq.lower(): dq for dq in DATA_QUALITY_VALUES}

    def collect(self, model):
        if model.id == 0:
            return
        quality = model.data_quality
        if quality.lower() in self._keymap:
            key = self._keymap[quality.lower()]
            self._data_quality[key] = self._data_quality[key] + 1
        else:
            print(f"Did not find quality: '{quality}' (id={model.id}). Keys: {self._data_quality.keys()}")

    def __str__(self):
        # print(self._data_quality)
        totals = sum(self._data_quality.values())
        lines = "\n".join(sorted([f"  {key.ljust(25)} = {value: >10,}" for key, value in self._data_quality.items()]))
        return f"Total: {totals} entries.\n{lines}"
