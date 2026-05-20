"""
奇波图片爬虫
从 https://wiki.biligame.com/ap/奇波一览 获取所有奇波子页面，
并下载每个子页面中的 kibo-pixel-box / animation.kibo-pixel / tab-content.kibo-frame 的图片。

依赖：pip install requests beautifulsoup4
"""

import os
import re
import time
import hashlib
import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin, urlparse, unquote
from concurrent.futures import ThreadPoolExecutor, as_completed

# ── 配置 ────────────────────────────────────────────────────────────────
BASE_URL    = "https://wiki.biligame.com"
LIST_URL    = "https://wiki.biligame.com/ap/%E5%A5%87%E6%B3%A2%E4%B8%80%E8%A7%88"
OUTPUT_DIR  = "kibo_images"
DELAY       = 0.4          # 每次请求间隔（秒），请勿设得太小
MAX_WORKERS = 4            # 并发下载线程数

HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/124.0.0.0 Safari/537.36"
    ),
    "Referer": BASE_URL,
    "Accept-Language": "zh-CN,zh;q=0.9",
}

# 目标 CSS 选择器（取这六个容器内的所有 img）
TARGET_SELECTORS = [
    ".kibo-pixel-box",
    ".animation.kibo-pixel",
    ".tab-content.kibo-frame",
    ".kibo-frame",
    ".tab-content.kibo-fore",
    ".tab-content",
]
# ────────────────────────────────────────────────────────────────────────


def safe_filename(name: str) -> str:
    """将字符串处理为合法文件/目录名"""
    return re.sub(r'[<>:"/\\|?*\x00-\x1f]', "_", name).strip("_. ")


def get_soup(url: str, retries: int = 3) -> BeautifulSoup | None:
    """获取页面并解析，自动重试"""
    for attempt in range(retries):
        try:
            resp = requests.get(url, headers=HEADERS, timeout=15)
            resp.raise_for_status()
            resp.encoding = resp.apparent_encoding or "utf-8"
            return BeautifulSoup(resp.text, "html.parser")
        except Exception as exc:
            print(f"  [WARN] 请求失败 ({attempt+1}/{retries}): {url} — {exc}")
            time.sleep(2 ** attempt)
    return None


def collect_kibo_links() -> list[tuple[str, str]]:
    """
    解析列表页，返回 [(kibo名称, 完整URL), ...] 的去重列表。
    """
    print(f"正在获取奇波列表: {LIST_URL}")
    soup = get_soup(LIST_URL)
    if soup is None:
        raise RuntimeError("无法获取奇波列表页")

    seen: set[str] = set()
    results: list[tuple[str, str]] = []

    for div in soup.select("div.divsort.ap-kibo-child"):
        a_tag = div.find("a", href=True)
        if not a_tag:
            continue
        href: str = a_tag["href"].strip()
        if not href or href.startswith("javascript"):
            continue
        full_url = href if href.startswith("http") else BASE_URL + href
        name = unquote(urlparse(full_url).path.rstrip("/").split("/")[-1])
        if full_url not in seen:
            seen.add(full_url)
            results.append((name, full_url))

    print(f"共找到 {len(results)} 个奇波条目")
    return results


def collect_image_urls(soup: BeautifulSoup, page_url: str) -> list[str]:
    """
    从解析好的页面中提取目标容器内的所有图片 URL（去重）。
    同时处理 src / data-src / data-original 三种懒加载写法。
    """
    found: list[str] = []
    seen: set[str] = set()

    for selector in TARGET_SELECTORS:
        for container in soup.select(selector):
            for img in container.find_all("img"):
                raw = (
                    img.get("src")
                    or img.get("data-src")
                    or img.get("data-original")
                    or ""
                ).strip()
                if not raw:
                    continue
                abs_url = raw if raw.startswith("http") else urljoin(page_url, raw)
                if abs_url not in seen:
                    seen.add(abs_url)
                    found.append(abs_url)

    return found


def download_image(img_url: str, save_dir: str) -> str:
    """
    下载单张图片到 save_dir，返回结果描述字符串。
    文件名从 URL 路径中提取，冲突时追加哈希后缀。
    """
    parsed   = urlparse(img_url)
    raw_name = unquote(parsed.path.split("/")[-1])
    filename = safe_filename(raw_name) or hashlib.md5(img_url.encode()).hexdigest()

    # 若同名文件已存在，直接跳过
    save_path = os.path.join(save_dir, filename)
    if os.path.exists(save_path):
        return f"  SKIP  {filename}"

    try:
        resp = requests.get(img_url, headers=HEADERS, timeout=20, stream=True)
        resp.raise_for_status()
        with open(save_path, "wb") as fout:
            for chunk in resp.iter_content(chunk_size=8192):
                fout.write(chunk)
        return f"  OK    {filename}"
    except Exception as exc:
        return f"  FAIL  {img_url} — {exc}"


def process_kibo(name: str, url: str, index: int, total: int) -> None:
    """处理单个奇波页面：解析 → 找图 → 下载"""
    print(f"\n[{index}/{total}] {name}  {url}")

    save_dir = os.path.join(OUTPUT_DIR, safe_filename(name))
    os.makedirs(save_dir, exist_ok=True)

    time.sleep(DELAY)
    soup = get_soup(url)
    if soup is None:
        print("  [ERROR] 页面获取失败，跳过")
        return

    img_urls = collect_image_urls(soup, url)
    print(f"  找到 {len(img_urls)} 张图片")

    if not img_urls:
        return

    # 并发下载
    with ThreadPoolExecutor(max_workers=MAX_WORKERS) as pool:
        futures = {pool.submit(download_image, u, save_dir): u for u in img_urls}
        for fut in as_completed(futures):
            print(fut.result())


def main() -> None:
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    try:
        kibo_list = collect_kibo_links()
    except RuntimeError as e:
        print(f"[FATAL] {e}")
        return

    total = len(kibo_list)
    for i, (name, url) in enumerate(kibo_list, start=1):
        process_kibo(name, url, i, total)

    print(f"\n完成！图片已保存到 ./{OUTPUT_DIR}/")


if __name__ == "__main__":
    main()
