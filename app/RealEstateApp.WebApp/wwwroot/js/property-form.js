(() => {
  "use strict";

  const MAX_IMAGES = 4;
  const MAX_BYTES = 15 * 1024 * 1024;
  const ALLOWED_TYPES = new Set(["image/jpeg", "image/png", "image/webp"]);

  document.querySelectorAll("[data-property-form]").forEach((form) => {
    const description = form.querySelector("[data-property-description]");
    const descriptionCount = form.querySelector("[data-description-count]");
    const price = form.querySelector("[data-property-price]");
    const pricePreview = form.querySelector("[data-price-preview]");

    const updateDescriptionCount = () => {
      if (description && descriptionCount) descriptionCount.textContent = `${description.value.length} / 2000`;
    };
    const updatePrice = () => {
      if (!price || !pricePreview) return;
      const value = Number(price.value);
      pricePreview.textContent = Number.isFinite(value)
        ? new Intl.NumberFormat("es-DO", { style: "currency", currency: "DOP" }).format(value)
        : "RD$ 0.00";
    };

    description?.addEventListener("input", updateDescriptionCount);
    price?.addEventListener("input", updatePrice);
    updateDescriptionCount();
    updatePrice();

    form.querySelectorAll("[data-image-uploader]").forEach((uploader) => {
      const input = uploader.querySelector("[data-image-input]");
      const dropZone = uploader.querySelector("[data-drop-zone]");
      const previews = uploader.querySelector("[data-image-previews]");
      const status = uploader.querySelector("[data-image-status]");
      const error = uploader.querySelector("[data-image-error]");
      const removalInputs = [...uploader.querySelectorAll("[data-remove-existing]")];
      let selectedFiles = [];

      const activeExistingCount = () => removalInputs.filter((item) => !item.checked).length;
      const totalCount = () => activeExistingCount() + selectedFiles.length;
      const setError = (message = "") => { if (error) error.textContent = message; };
      const syncInput = () => {
        const transfer = new DataTransfer();
        selectedFiles.forEach((file) => transfer.items.add(file));
        input.files = transfer.files;
      };
      const render = () => {
        previews.replaceChildren();
        selectedFiles.forEach((file, index) => {
          const card = document.createElement("div");
          card.className = "group relative aspect-square overflow-hidden rounded-2xl border border-default bg-[var(--color-bg-muted)]";
          const image = document.createElement("img");
          const url = URL.createObjectURL(file);
          image.src = url;
          image.alt = `Vista previa ${index + 1}: ${file.name}`;
          image.className = "h-full w-full object-cover";
          image.addEventListener("load", () => URL.revokeObjectURL(url), { once: true });
          const remove = document.createElement("button");
          remove.type = "button";
          remove.className = "absolute right-2 top-2 flex h-8 w-8 items-center justify-center rounded-full bg-gray-950/75 text-white shadow transition hover:bg-danger-700";
          remove.setAttribute("aria-label", `Quitar ${file.name}`);
          remove.innerHTML = '<i data-lucide="x" class="h-4 w-4"></i>';
          remove.addEventListener("click", () => {
            selectedFiles.splice(index, 1);
            syncInput();
            render();
          });
          card.append(image, remove);
          previews.append(card);
        });
        if (status) status.textContent = `${totalCount()} de ${MAX_IMAGES} imágenes`;
        if (window.lucide) window.lucide.createIcons();
      };
      const addFiles = (files) => {
        setError();
        for (const file of files) {
          if (!ALLOWED_TYPES.has(file.type)) { setError("Solo se permiten imágenes JPG, PNG o WebP."); continue; }
          if (file.size > MAX_BYTES) { setError(`${file.name} excede 15 MB.`); continue; }
          if (totalCount() >= MAX_IMAGES) { setError("Solo se permiten hasta 4 imágenes en total."); break; }
          if (!selectedFiles.some((item) => item.name === file.name && item.size === file.size)) selectedFiles.push(file);
        }
        syncInput();
        render();
      };

      input?.addEventListener("change", () => addFiles(input.files));
      removalInputs.forEach((checkbox) => checkbox.addEventListener("change", () => { setError(); render(); }));
      ["dragenter", "dragover"].forEach((name) => dropZone?.addEventListener(name, (event) => { event.preventDefault(); dropZone.classList.add("border-brand-500", "bg-brand-50"); }));
      ["dragleave", "drop"].forEach((name) => dropZone?.addEventListener(name, (event) => { event.preventDefault(); dropZone.classList.remove("border-brand-500", "bg-brand-50"); }));
      dropZone?.addEventListener("drop", (event) => addFiles(event.dataTransfer.files));
      dropZone?.addEventListener("keydown", (event) => { if (event.key === "Enter" || event.key === " ") { event.preventDefault(); input.click(); } });
      render();

      form.addEventListener("submit", (event) => {
        if (totalCount() < 1 || totalCount() > MAX_IMAGES) {
          event.preventDefault();
          setError(totalCount() < 1 ? "Debe conservar o cargar al menos una imagen." : "Solo se permiten hasta 4 imágenes en total.");
          dropZone?.focus();
        }
      });
    });

    form.addEventListener("submit", (event) => {
      if (event.defaultPrevented) return;
      const improvements = form.querySelectorAll('[name="ImprovementIds"]:checked');
      const improvementError = form.querySelector("[data-improvement-error]");
      if (improvements.length === 0) {
        event.preventDefault();
        if (improvementError) improvementError.textContent = "Debe seleccionar al menos una mejora.";
        form.querySelector('[name="ImprovementIds"]')?.focus();
        form.querySelector("[data-improvements]")?.scrollIntoView({
          behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "auto" : "smooth",
          block: "center",
        });
        return;
      }
      const submit = form.querySelector('button[type="submit"]');
      if (submit) { submit.disabled = true; submit.setAttribute("aria-busy", "true"); }
    });

    form.querySelectorAll('[name="ImprovementIds"]').forEach((checkbox) => {
      checkbox.addEventListener("change", () => {
        const error = form.querySelector("[data-improvement-error]");
        if (error && form.querySelector('[name="ImprovementIds"]:checked')) error.textContent = "";
      });
    });
  });
})();
