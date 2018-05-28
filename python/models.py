from typing import List


class Label:
    def __init__(self):
        self.id: int = 0
        self.data_quality: str = None
        self.name: str = None
        self.contact_info: str = None
        self.profile: str = None
        self.urls: List[str] = []
        self.sublabels: List[str] = []
        self.parent_label: str = None


class Artist:
    def __init__(self):
        self.id: int = 0
        self.data_quality: str = None
        self.name: str = None
        self.real_name: str = None
        self.profile: str = None
        self.urls: List[str] = []
        self.aliases: List[str] = []
        self.name_variations: List[str] = []
        self.members: List[str] = []
        self.groups: List[str] = []
