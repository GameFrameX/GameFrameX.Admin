#!/usr/bin/env python3
"""Validate the C405 independent Admin compose topology."""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from pathlib import Path


EXPECTED_SERVICES = {"admin-web", "admin-client", "admin-hub", "pg"}
EXPECTED_PORTS = {
    "admin-web": "20802:20802",
    "admin-client": "20808:20808",
    "admin-hub": "20806:20806",
    "pg": "5432:5432",
}
SENSITIVE_LITERAL = re.compile(
    r"(?:password|secret|api[_-]?key|private[_-]?key)\s*[:=]\s*['\"]?(?!\$\{)[^\s,'\"]+",
    re.IGNORECASE,
)


def render_compose(compose_file: Path) -> dict:
    environment = os.environ.copy()
    environment.setdefault("POSTGRES_PASSWORD", "structure-check-only")
    process = subprocess.run(
        ["docker", "compose", "-f", str(compose_file), "config", "--format", "json"],
        check=False,
        capture_output=True,
        text=True,
        env=environment,
    )
    if process.returncode != 0:
        raise RuntimeError(process.stderr.strip() or "docker compose config failed")
    return json.loads(process.stdout)


def check_topology(rendered: dict) -> list[str]:
    services = rendered.get("services", {})
    errors: list[str] = []
    actual_services = set(services)
    if actual_services != EXPECTED_SERVICES:
        errors.append(f"services must be {sorted(EXPECTED_SERVICES)}, got {sorted(actual_services)}")

    for service_name, port in EXPECTED_PORTS.items():
        service_ports = {
            f"{item.get('published')}:{item.get('target')}"
            for item in services.get(service_name, {}).get("ports", [])
        }
        if port not in service_ports:
            errors.append(f"{service_name} must publish {port}")

    if services.get("admin-web", {}).get("depends_on", {}).get("pg", {}).get("condition") != "service_healthy":
        errors.append("admin-web must wait for healthy pg")
    for service_name in ("admin-client", "admin-hub"):
        if services.get(service_name, {}).get("depends_on", {}).get("admin-web", {}).get("condition") != "service_healthy":
            errors.append(f"{service_name} must wait for healthy admin-web")

    for service_name, service in services.items():
        for key, value in service.get("environment", {}).items():
            if key.upper().endswith(("PASSWORD", "SECRET", "API_KEY")) and value not in (None, ""):
                # Compose has already interpolated values; the check only rejects source literals below.
                continue
    return errors


def check_source(compose_file: Path) -> list[str]:
    source = compose_file.read_text(encoding="utf-8")
    return [
        f"possible credential literal at source line {line_number}"
        for line_number, line in enumerate(source.splitlines(), start=1)
        if SENSITIVE_LITERAL.search(re.sub(r"\$\{[^}]*\}", "${}", line))
    ]


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("compose_file", nargs="?", type=Path, default=Path("docker-compose.admin.yml"))
    arguments = parser.parse_args()
    compose_file = arguments.compose_file.resolve()
    try:
        errors = check_source(compose_file)
        errors.extend(check_topology(render_compose(compose_file)))
    except (OSError, RuntimeError, json.JSONDecodeError) as error:
        print(f"FAIL: {error}", file=sys.stderr)
        return 1
    if errors:
        for error in errors:
            print(f"FAIL: {error}", file=sys.stderr)
        return 1
    print(f"PASS: {compose_file}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
