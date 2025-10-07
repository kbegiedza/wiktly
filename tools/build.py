from pathlib import Path
# from docker import DockerClient
from datetime import datetime

REPO_ROOT = Path(__file__).parent.parent
API_ROOT = REPO_ROOT / "services/api"
SOLUTION_NAME = "Wiktly"
API_PROJECT_NAME = "Wiktly.API"

OWNER_TAG = "kbegiedza"

def build_api():
    print("Building API...")
    print(REPO_ROOT)

    now = datetime.now().isoformat()

    print(now)

    # docker = DockerClient()
    # docker.images.build(
    #     path=str(API_ROOT),
    #     dockerfile="Dockerfile",
    #     tag=f"{OWNER_TAG}/{SOLUTION_NAME.lower()}/{API_PROJECT_NAME.lower()}:latest",
    #     rm=True,
    # )

    pass

if __name__ == "__main__":
    build_api()
