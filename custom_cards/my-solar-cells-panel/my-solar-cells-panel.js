function t(t,e,s,i){var r,a=arguments.length,o=a<3?e:null===i?i=Object.getOwnPropertyDescriptor(e,s):i;if("object"==typeof Reflect&&"function"==typeof Reflect.decorate)o=Reflect.decorate(t,e,s,i);else for(var n=t.length-1;n>=0;n--)(r=t[n])&&(o=(a<3?r(o):a>3?r(e,s,o):r(e,s))||o);return a>3&&o&&Object.defineProperty(e,s,o),o}"function"==typeof SuppressedError&&SuppressedError;
/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const e=globalThis,s=e.ShadowRoot&&(void 0===e.ShadyCSS||e.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,i=Symbol(),r=new WeakMap;let a=class{constructor(t,e,s){if(this._$cssResult$=!0,s!==i)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=e}get styleSheet(){let t=this.o;const e=this.t;if(s&&void 0===t){const s=void 0!==e&&1===e.length;s&&(t=r.get(e)),void 0===t&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),s&&r.set(e,t))}return t}toString(){return this.cssText}};const o=(t,...e)=>{const s=1===t.length?t[0]:e.reduce((e,s,i)=>e+(t=>{if(!0===t._$cssResult$)return t.cssText;if("number"==typeof t)return t;throw Error("Value passed to 'css' function must be a 'css' function result: "+t+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(s)+t[i+1],t[0]);return new a(s,t,i)},n=s?t=>t:t=>t instanceof CSSStyleSheet?(t=>{let e="";for(const s of t.cssRules)e+=s.cssText;return(t=>new a("string"==typeof t?t:t+"",void 0,i))(e)})(t):t,{is:d,defineProperty:l,getOwnPropertyDescriptor:h,getOwnPropertyNames:c,getOwnPropertySymbols:p,getPrototypeOf:u}=Object,_=globalThis,g=_.trustedTypes,y=g?g.emptyScript:"",v=_.reactiveElementPolyfillSupport,f=(t,e)=>t,m={toAttribute(t,e){switch(e){case Boolean:t=t?y:null;break;case Object:case Array:t=null==t?t:JSON.stringify(t)}return t},fromAttribute(t,e){let s=t;switch(e){case Boolean:s=null!==t;break;case Number:s=null===t?null:Number(t);break;case Object:case Array:try{s=JSON.parse(t)}catch(t){s=null}}return s}},b=(t,e)=>!d(t,e),$={attribute:!0,type:String,converter:m,reflect:!1,useDefault:!1,hasChanged:b};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */Symbol.metadata??=Symbol("metadata"),_.litPropertyMetadata??=new WeakMap;let x=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??=[]).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,e=$){if(e.state&&(e.attribute=!1),this._$Ei(),this.prototype.hasOwnProperty(t)&&((e=Object.create(e)).wrapped=!0),this.elementProperties.set(t,e),!e.noAccessor){const s=Symbol(),i=this.getPropertyDescriptor(t,s,e);void 0!==i&&l(this.prototype,t,i)}}static getPropertyDescriptor(t,e,s){const{get:i,set:r}=h(this.prototype,t)??{get(){return this[e]},set(t){this[e]=t}};return{get:i,set(e){const a=i?.call(this);r?.call(this,e),this.requestUpdate(t,a,s)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??$}static _$Ei(){if(this.hasOwnProperty(f("elementProperties")))return;const t=u(this);t.finalize(),void 0!==t.l&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(f("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(f("properties"))){const t=this.properties,e=[...c(t),...p(t)];for(const s of e)this.createProperty(s,t[s])}const t=this[Symbol.metadata];if(null!==t){const e=litPropertyMetadata.get(t);if(void 0!==e)for(const[t,s]of e)this.elementProperties.set(t,s)}this._$Eh=new Map;for(const[t,e]of this.elementProperties){const s=this._$Eu(t,e);void 0!==s&&this._$Eh.set(s,t)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const e=[];if(Array.isArray(t)){const s=new Set(t.flat(1/0).reverse());for(const t of s)e.unshift(n(t))}else void 0!==t&&e.push(n(t));return e}static _$Eu(t,e){const s=e.attribute;return!1===s?void 0:"string"==typeof s?s:"string"==typeof t?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){this._$ES=new Promise(t=>this.enableUpdating=t),this._$AL=new Map,this._$E_(),this.requestUpdate(),this.constructor.l?.forEach(t=>t(this))}addController(t){(this._$EO??=new Set).add(t),void 0!==this.renderRoot&&this.isConnected&&t.hostConnected?.()}removeController(t){this._$EO?.delete(t)}_$E_(){const t=new Map,e=this.constructor.elementProperties;for(const s of e.keys())this.hasOwnProperty(s)&&(t.set(s,this[s]),delete this[s]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return((t,i)=>{if(s)t.adoptedStyleSheets=i.map(t=>t instanceof CSSStyleSheet?t:t.styleSheet);else for(const s of i){const i=document.createElement("style"),r=e.litNonce;void 0!==r&&i.setAttribute("nonce",r),i.textContent=s.cssText,t.appendChild(i)}})(t,this.constructor.elementStyles),t}connectedCallback(){this.renderRoot??=this.createRenderRoot(),this.enableUpdating(!0),this._$EO?.forEach(t=>t.hostConnected?.())}enableUpdating(t){}disconnectedCallback(){this._$EO?.forEach(t=>t.hostDisconnected?.())}attributeChangedCallback(t,e,s){this._$AK(t,s)}_$ET(t,e){const s=this.constructor.elementProperties.get(t),i=this.constructor._$Eu(t,s);if(void 0!==i&&!0===s.reflect){const r=(void 0!==s.converter?.toAttribute?s.converter:m).toAttribute(e,s.type);this._$Em=t,null==r?this.removeAttribute(i):this.setAttribute(i,r),this._$Em=null}}_$AK(t,e){const s=this.constructor,i=s._$Eh.get(t);if(void 0!==i&&this._$Em!==i){const t=s.getPropertyOptions(i),r="function"==typeof t.converter?{fromAttribute:t.converter}:void 0!==t.converter?.fromAttribute?t.converter:m;this._$Em=i;const a=r.fromAttribute(e,t.type);this[i]=a??this._$Ej?.get(i)??a,this._$Em=null}}requestUpdate(t,e,s,i=!1,r){if(void 0!==t){const a=this.constructor;if(!1===i&&(r=this[t]),s??=a.getPropertyOptions(t),!((s.hasChanged??b)(r,e)||s.useDefault&&s.reflect&&r===this._$Ej?.get(t)&&!this.hasAttribute(a._$Eu(t,s))))return;this.C(t,e,s)}!1===this.isUpdatePending&&(this._$ES=this._$EP())}C(t,e,{useDefault:s,reflect:i,wrapped:r},a){s&&!(this._$Ej??=new Map).has(t)&&(this._$Ej.set(t,a??e??this[t]),!0!==r||void 0!==a)||(this._$AL.has(t)||(this.hasUpdated||s||(e=void 0),this._$AL.set(t,e)),!0===i&&this._$Em!==t&&(this._$Eq??=new Set).add(t))}async _$EP(){this.isUpdatePending=!0;try{await this._$ES}catch(t){Promise.reject(t)}const t=this.scheduleUpdate();return null!=t&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??=this.createRenderRoot(),this._$Ep){for(const[t,e]of this._$Ep)this[t]=e;this._$Ep=void 0}const t=this.constructor.elementProperties;if(t.size>0)for(const[e,s]of t){const{wrapped:t}=s,i=this[e];!0!==t||this._$AL.has(e)||void 0===i||this.C(e,void 0,s,i)}}let t=!1;const e=this._$AL;try{t=this.shouldUpdate(e),t?(this.willUpdate(e),this._$EO?.forEach(t=>t.hostUpdate?.()),this.update(e)):this._$EM()}catch(e){throw t=!1,this._$EM(),e}t&&this._$AE(e)}willUpdate(t){}_$AE(t){this._$EO?.forEach(t=>t.hostUpdated?.()),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EM(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Eq&&=this._$Eq.forEach(t=>this._$ET(t,this[t])),this._$EM()}updated(t){}firstUpdated(t){}};x.elementStyles=[],x.shadowRootOptions={mode:"open"},x[f("elementProperties")]=new Map,x[f("finalized")]=new Map,v?.({ReactiveElement:x}),(_.reactiveElementVersions??=[]).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const w=globalThis,A=t=>t,S=w.trustedTypes,k=S?S.createPolicy("lit-html",{createHTML:t=>t}):void 0,E="$lit$",P=`lit$${Math.random().toFixed(9).slice(2)}$`,T="?"+P,C=`<${T}>`,D=document,I=()=>D.createComment(""),O=t=>null===t||"object"!=typeof t&&"function"!=typeof t,R=Array.isArray,U="[ \t\n\f\r]",Y=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,M=/-->/g,N=/>/g,H=RegExp(`>|${U}(?:([^\\s"'>=/]+)(${U}*=${U}*(?:[^ \t\n\f\r"'\`<>=]|("|')|))|$)`,"g"),z=/'/g,F=/"/g,W=/^(?:script|style|textarea|title)$/i,L=(t=>(e,...s)=>({_$litType$:t,strings:e,values:s}))(1),V=Symbol.for("lit-noChange"),j=Symbol.for("lit-nothing"),q=new WeakMap,B=D.createTreeWalker(D,129);function K(t,e){if(!R(t)||!t.hasOwnProperty("raw"))throw Error("invalid template strings array");return void 0!==k?k.createHTML(e):e}const G=(t,e)=>{const s=t.length-1,i=[];let r,a=2===e?"<svg>":3===e?"<math>":"",o=Y;for(let e=0;e<s;e++){const s=t[e];let n,d,l=-1,h=0;for(;h<s.length&&(o.lastIndex=h,d=o.exec(s),null!==d);)h=o.lastIndex,o===Y?"!--"===d[1]?o=M:void 0!==d[1]?o=N:void 0!==d[2]?(W.test(d[2])&&(r=RegExp("</"+d[2],"g")),o=H):void 0!==d[3]&&(o=H):o===H?">"===d[0]?(o=r??Y,l=-1):void 0===d[1]?l=-2:(l=o.lastIndex-d[2].length,n=d[1],o=void 0===d[3]?H:'"'===d[3]?F:z):o===F||o===z?o=H:o===M||o===N?o=Y:(o=H,r=void 0);const c=o===H&&t[e+1].startsWith("/>")?" ":"";a+=o===Y?s+C:l>=0?(i.push(n),s.slice(0,l)+E+s.slice(l)+P+c):s+P+(-2===l?e:c)}return[K(t,a+(t[s]||"<?>")+(2===e?"</svg>":3===e?"</math>":"")),i]};class J{constructor({strings:t,_$litType$:e},s){let i;this.parts=[];let r=0,a=0;const o=t.length-1,n=this.parts,[d,l]=G(t,e);if(this.el=J.createElement(d,s),B.currentNode=this.el.content,2===e||3===e){const t=this.el.content.firstChild;t.replaceWith(...t.childNodes)}for(;null!==(i=B.nextNode())&&n.length<o;){if(1===i.nodeType){if(i.hasAttributes())for(const t of i.getAttributeNames())if(t.endsWith(E)){const e=l[a++],s=i.getAttribute(t).split(P),o=/([.?@])?(.*)/.exec(e);n.push({type:1,index:r,name:o[2],strings:s,ctor:"."===o[1]?et:"?"===o[1]?st:"@"===o[1]?it:tt}),i.removeAttribute(t)}else t.startsWith(P)&&(n.push({type:6,index:r}),i.removeAttribute(t));if(W.test(i.tagName)){const t=i.textContent.split(P),e=t.length-1;if(e>0){i.textContent=S?S.emptyScript:"";for(let s=0;s<e;s++)i.append(t[s],I()),B.nextNode(),n.push({type:2,index:++r});i.append(t[e],I())}}}else if(8===i.nodeType)if(i.data===T)n.push({type:2,index:r});else{let t=-1;for(;-1!==(t=i.data.indexOf(P,t+1));)n.push({type:7,index:r}),t+=P.length-1}r++}}static createElement(t,e){const s=D.createElement("template");return s.innerHTML=t,s}}function Z(t,e,s=t,i){if(e===V)return e;let r=void 0!==i?s._$Co?.[i]:s._$Cl;const a=O(e)?void 0:e._$litDirective$;return r?.constructor!==a&&(r?._$AO?.(!1),void 0===a?r=void 0:(r=new a(t),r._$AT(t,s,i)),void 0!==i?(s._$Co??=[])[i]=r:s._$Cl=r),void 0!==r&&(e=Z(t,r._$AS(t,e.values),r,i)),e}class Q{constructor(t,e){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=e}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:e},parts:s}=this._$AD,i=(t?.creationScope??D).importNode(e,!0);B.currentNode=i;let r=B.nextNode(),a=0,o=0,n=s[0];for(;void 0!==n;){if(a===n.index){let e;2===n.type?e=new X(r,r.nextSibling,this,t):1===n.type?e=new n.ctor(r,n.name,n.strings,this,t):6===n.type&&(e=new rt(r,this,t)),this._$AV.push(e),n=s[++o]}a!==n?.index&&(r=B.nextNode(),a++)}return B.currentNode=D,i}p(t){let e=0;for(const s of this._$AV)void 0!==s&&(void 0!==s.strings?(s._$AI(t,s,e),e+=s.strings.length-2):s._$AI(t[e])),e++}}class X{get _$AU(){return this._$AM?._$AU??this._$Cv}constructor(t,e,s,i){this.type=2,this._$AH=j,this._$AN=void 0,this._$AA=t,this._$AB=e,this._$AM=s,this.options=i,this._$Cv=i?.isConnected??!0}get parentNode(){let t=this._$AA.parentNode;const e=this._$AM;return void 0!==e&&11===t?.nodeType&&(t=e.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,e=this){t=Z(this,t,e),O(t)?t===j||null==t||""===t?(this._$AH!==j&&this._$AR(),this._$AH=j):t!==this._$AH&&t!==V&&this._(t):void 0!==t._$litType$?this.$(t):void 0!==t.nodeType?this.T(t):(t=>R(t)||"function"==typeof t?.[Symbol.iterator])(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==j&&O(this._$AH)?this._$AA.nextSibling.data=t:this.T(D.createTextNode(t)),this._$AH=t}$(t){const{values:e,_$litType$:s}=t,i="number"==typeof s?this._$AC(t):(void 0===s.el&&(s.el=J.createElement(K(s.h,s.h[0]),this.options)),s);if(this._$AH?._$AD===i)this._$AH.p(e);else{const t=new Q(i,this),s=t.u(this.options);t.p(e),this.T(s),this._$AH=t}}_$AC(t){let e=q.get(t.strings);return void 0===e&&q.set(t.strings,e=new J(t)),e}k(t){R(this._$AH)||(this._$AH=[],this._$AR());const e=this._$AH;let s,i=0;for(const r of t)i===e.length?e.push(s=new X(this.O(I()),this.O(I()),this,this.options)):s=e[i],s._$AI(r),i++;i<e.length&&(this._$AR(s&&s._$AB.nextSibling,i),e.length=i)}_$AR(t=this._$AA.nextSibling,e){for(this._$AP?.(!1,!0,e);t!==this._$AB;){const e=A(t).nextSibling;A(t).remove(),t=e}}setConnected(t){void 0===this._$AM&&(this._$Cv=t,this._$AP?.(t))}}class tt{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,e,s,i,r){this.type=1,this._$AH=j,this._$AN=void 0,this.element=t,this.name=e,this._$AM=i,this.options=r,s.length>2||""!==s[0]||""!==s[1]?(this._$AH=Array(s.length-1).fill(new String),this.strings=s):this._$AH=j}_$AI(t,e=this,s,i){const r=this.strings;let a=!1;if(void 0===r)t=Z(this,t,e,0),a=!O(t)||t!==this._$AH&&t!==V,a&&(this._$AH=t);else{const i=t;let o,n;for(t=r[0],o=0;o<r.length-1;o++)n=Z(this,i[s+o],e,o),n===V&&(n=this._$AH[o]),a||=!O(n)||n!==this._$AH[o],n===j?t=j:t!==j&&(t+=(n??"")+r[o+1]),this._$AH[o]=n}a&&!i&&this.j(t)}j(t){t===j?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}class et extends tt{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===j?void 0:t}}class st extends tt{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==j)}}class it extends tt{constructor(t,e,s,i,r){super(t,e,s,i,r),this.type=5}_$AI(t,e=this){if((t=Z(this,t,e,0)??j)===V)return;const s=this._$AH,i=t===j&&s!==j||t.capture!==s.capture||t.once!==s.once||t.passive!==s.passive,r=t!==j&&(s===j||i);i&&this.element.removeEventListener(this.name,this,s),r&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){"function"==typeof this._$AH?this._$AH.call(this.options?.host??this.element,t):this._$AH.handleEvent(t)}}class rt{constructor(t,e,s){this.element=t,this.type=6,this._$AN=void 0,this._$AM=e,this.options=s}get _$AU(){return this._$AM._$AU}_$AI(t){Z(this,t)}}const at=w.litHtmlPolyfillSupport;at?.(J,X),(w.litHtmlVersions??=[]).push("3.3.2");const ot=globalThis;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */class nt extends x{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){const t=super.createRenderRoot();return this.renderOptions.renderBefore??=t.firstChild,t}update(t){const e=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=((t,e,s)=>{const i=s?.renderBefore??e;let r=i._$litPart$;if(void 0===r){const t=s?.renderBefore??null;i._$litPart$=r=new X(e.insertBefore(I(),t),t,void 0,s??{})}return r._$AI(t),r})(e,this.renderRoot,this.renderOptions)}connectedCallback(){super.connectedCallback(),this._$Do?.setConnected(!0)}disconnectedCallback(){super.disconnectedCallback(),this._$Do?.setConnected(!1)}render(){return V}}nt._$litElement$=!0,nt.finalized=!0,ot.litElementHydrateSupport?.({LitElement:nt});const dt=ot.litElementPolyfillSupport;dt?.({LitElement:nt}),(ot.litElementVersions??=[]).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const lt=t=>(e,s)=>{void 0!==s?s.addInitializer(()=>{customElements.define(t,e)}):customElements.define(t,e)},ht={attribute:!0,type:String,converter:m,reflect:!1,hasChanged:b},ct=(t=ht,e,s)=>{const{kind:i,metadata:r}=s;let a=globalThis.litPropertyMetadata.get(r);if(void 0===a&&globalThis.litPropertyMetadata.set(r,a=new Map),"setter"===i&&((t=Object.create(t)).wrapped=!0),a.set(s.name,t),"accessor"===i){const{name:i}=s;return{set(s){const r=e.get.call(this);e.set.call(this,s),this.requestUpdate(i,r,t,!0,s)},init(e){return void 0!==e&&this.C(i,void 0,t,e),e}}}if("setter"===i){const{name:i}=s;return function(s){const r=this[i];e.call(this,s),this.requestUpdate(i,r,t,!0,s)}}throw Error("Unsupported decorator location: "+i)};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function pt(t){return(e,s)=>"object"==typeof s?ct(t,e,s):((t,e,s)=>{const i=e.hasOwnProperty(s);return e.constructor.createProperty(s,t),i?Object.getOwnPropertyDescriptor(e,s):void 0})(t,e,s)}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function ut(t){return pt({...t,state:!0,attribute:!1})}const _t=o`
  :host {
    display: block;
    padding: 16px;
    --mdc-theme-primary: var(--primary-color);
  }

  .header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

  .header h1 {
    margin: 0;
    font-size: 1.5em;
    font-weight: 400;
    color: var(--primary-text-color);
  }

  .tabs {
    display: flex;
    border-bottom: 1px solid var(--divider-color);
    margin-bottom: 16px;
  }

  .tab {
    padding: 12px 20px;
    cursor: pointer;
    border-bottom: 2px solid transparent;
    color: var(--secondary-text-color);
    font-size: 0.95em;
    font-weight: 500;
    transition: color 0.2s, border-color 0.2s;
    background: none;
    border-top: none;
    border-left: none;
    border-right: none;
  }

  .tab:hover {
    color: var(--primary-text-color);
  }

  .tab.active {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
  }
`,gt=o`
  .card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
    margin-bottom: 16px;
  }

  .card h3 {
    margin: 0 0 12px 0;
    font-size: 1.1em;
    font-weight: 500;
    color: var(--primary-text-color);
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 12px;
  }

  .stat-item {
    text-align: center;
    padding: 12px 8px;
    border-radius: 8px;
    background: var(--primary-background-color);
  }

  .stat-label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
    margin-bottom: 4px;
  }

  .stat-value {
    font-size: 1.3em;
    font-weight: 600;
    color: var(--primary-text-color);
  }
`,yt=o`
  .table-controls {
    display: flex;
    gap: 12px;
    align-items: flex-end;
    flex-wrap: wrap;
    margin-bottom: 16px;
  }

  .input-group {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .input-group label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
  }

  .input-group input {
    padding: 8px 12px;
    border: 1px solid var(--divider-color);
    border-radius: 6px;
    background: var(--card-background-color, white);
    color: var(--primary-text-color);
    font-size: 0.9em;
  }

  button.btn {
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.9em;
    font-weight: 500;
  }

  button.btn:hover {
    opacity: 0.9;
  }

  button.btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.85em;
  }

  th {
    text-align: left;
    padding: 8px 6px;
    border-bottom: 2px solid var(--divider-color);
    color: var(--secondary-text-color);
    font-weight: 500;
    white-space: nowrap;
  }

  td {
    padding: 6px;
    border-bottom: 1px solid var(--divider-color);
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 16px;
    margin-top: 12px;
    font-size: 0.9em;
    color: var(--secondary-text-color);
  }

  .summary-row {
    background: var(--primary-background-color);
    font-weight: 600;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`,vt=[{key:"own_use_kwh",label:"Own Use",color:"#4285f4",unit:"kWh"},{key:"sold_kwh",label:"Sold",color:"#34a853",unit:"kWh"},{key:"own_use_sek",label:"Own Use",color:"#8ab4f8",unit:"SEK"},{key:"sold_sek",label:"Sold",color:"#f4a742",unit:"SEK"}],ft=[{key:"today",label:"Today"},{key:"this_week",label:"This Week"},{key:"this_month",label:"This Month"},{key:"this_year",label:"This Year"}];let mt=class extends nt{render(){if(!this.data)return j;const t=this._getMax("kwh"),e=this._getMax("sek");return L`
      <div class="chart-container">
        ${ft.map(s=>this._renderPeriod(s,this.data[s.key],t,e))}
      </div>
      <div class="legend">
        ${vt.map(t=>L`
            <div class="legend-item">
              <div class="legend-swatch" style="background:${t.color}"></div>
              ${t.label} (${t.unit})
            </div>
          `)}
      </div>
    `}_renderPeriod(t,e,s,i){return L`
      <div class="period-column">
        <div class="period-label">${t.label}</div>
        <div class="bars">
          ${vt.map(t=>{const r=e[t.key],a="kWh"===t.unit?s:i;return L`
              <div class="bar-wrapper">
                <div
                  class="bar"
                  style="height:${a>0?r/a*100:0}%;background:${t.color}"
                ></div>
                <div class="bar-value">${this._formatValue(r,t.unit)}</div>
              </div>
            `})}
        </div>
      </div>
    `}_getMax(t){if(!this.data)return 0;const e="kwh"===t?["own_use_kwh","sold_kwh"]:["own_use_sek","sold_sek"];let s=0;for(const t of ft){const i=this.data[t.key];for(const t of e)i[t]>s&&(s=i[t])}return s}_formatValue(t,e){return t>=1e3?`${(t/1e3).toFixed(1)}k`:t>=10?t.toFixed(0):t.toFixed(1)}};mt.styles=o`
    :host {
      display: block;
    }

    .chart-container {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
    }

    @media (max-width: 600px) {
      .chart-container {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    .period-column {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .period-label {
      font-size: 0.85em;
      font-weight: 500;
      color: var(--primary-text-color);
      margin-bottom: 8px;
    }

    .bars {
      display: flex;
      gap: 4px;
      align-items: flex-end;
      height: 120px;
      width: 100%;
      justify-content: center;
    }

    .bar-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex: 1;
      max-width: 36px;
      height: 100%;
      justify-content: flex-end;
    }

    .bar {
      width: 100%;
      border-radius: 3px 3px 0 0;
      min-height: 2px;
      transition: height 0.3s ease;
    }

    .bar-value {
      font-size: 0.65em;
      color: var(--secondary-text-color);
      margin-top: 4px;
      white-space: nowrap;
      text-align: center;
    }

    .legend {
      display: flex;
      justify-content: center;
      gap: 16px;
      margin-top: 16px;
      flex-wrap: wrap;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.75em;
      color: var(--secondary-text-color);
    }

    .legend-swatch {
      width: 10px;
      height: 10px;
      border-radius: 2px;
    }
  `,t([pt({attribute:!1})],mt.prototype,"data",void 0),mt=t([lt("period-summary-chart")],mt);let bt=class extends nt{constructor(){super(...arguments),this.entryId="",this._data=null,this._periodData=null,this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._data&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const[t,e]=await Promise.all([this.hass.callWS({type:"my_solar_cells/get_overview",entry_id:this.entryId}),this.hass.callWS({type:"my_solar_cells/get_period_summaries",entry_id:this.entryId})]);this._data=t,this._periodData=e}catch(t){this._error=t.message||"Failed to fetch data"}this._loading=!1}}render(){if(this._loading)return L`<div class="loading">Loading overview...</div>`;if(this._error)return L`<div class="no-data">Error: ${this._error}</div>`;if(!this._data)return L`<div class="no-data">No data available</div>`;const t=this._data;return L`
      <div class="card">
        <h3>Database Summary</h3>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-label">Last Tibber Sync</div>
            <div class="stat-value">${t.last_tibber_sync?this._formatTimestamp(t.last_tibber_sync):"Never"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Hourly Records</div>
            <div class="stat-value">${t.hourly_record_count.toLocaleString()}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">First Record</div>
            <div class="stat-value">${t.first_timestamp?this._formatDate(t.first_timestamp):"N/A"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Last Record</div>
            <div class="stat-value">${t.last_timestamp?this._formatDate(t.last_timestamp):"N/A"}</div>
          </div>
        </div>
      </div>

      ${this._periodData?L`
            <div class="card">
              <h3>Energy Summary</h3>
              <period-summary-chart .data=${this._periodData}></period-summary-chart>
            </div>
          `:j}

      ${this._renderYearlyParams(t.yearly_params)}
    `}_renderYearlyParams(t){const e=Object.keys(t).sort();return 0===e.length?j:L`
      <div class="card">
        <h3>Yearly Financial Parameters</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Year</th>
                <th>Tax Reduction</th>
                <th>Grid Comp.</th>
                <th>Transfer Fee</th>
                <th>Energy Tax</th>
                <th>Installed kW</th>
              </tr>
            </thead>
            <tbody>
              ${e.map(e=>{const s=t[e];return L`
                  <tr>
                    <td>${e}</td>
                    <td>${this._fmt(s.tax_reduction)}</td>
                    <td>${this._fmt(s.grid_compensation)}</td>
                    <td>${this._fmt(s.transfer_fee)}</td>
                    <td>${this._fmt(s.energy_tax)}</td>
                    <td>${null!=s.installed_kw?s.installed_kw:"-"}</td>
                  </tr>
                `})}
            </tbody>
          </table>
        </div>
      </div>
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_formatTimestamp(t){try{return new Date(t).toLocaleString("sv-SE")}catch{return t}}_formatDate(t){try{return t.substring(0,10)}catch{return t}}};bt.styles=[gt,yt],t([pt({attribute:!1})],bt.prototype,"hass",void 0),t([pt()],bt.prototype,"entryId",void 0),t([ut()],bt.prototype,"_data",void 0),t([ut()],bt.prototype,"_periodData",void 0),t([ut()],bt.prototype,"_loading",void 0),t([ut()],bt.prototype,"_error",void 0),bt=t([lt("overview-view")],bt);const $t=50;let xt=class extends nt{constructor(){super(...arguments),this.entryId="",this._startDate="",this._endDate="",this._records=[],this._totalCount=0,this._offset=0,this._loading=!1,this._error=""}connectedCallback(){super.connectedCallback();const t=(new Date).toISOString().substring(0,10);this._startDate=t,this._endDate=t}render(){return L`
      <div class="card">
        <h3>Hourly Energy Records</h3>
        <div class="table-controls">
          <div class="input-group">
            <label>Start Date</label>
            <input
              type="date"
              .value=${this._startDate}
              @change=${t=>{this._startDate=t.target.value}}
            />
          </div>
          <div class="input-group">
            <label>End Date</label>
            <input
              type="date"
              .value=${this._endDate}
              @change=${t=>{this._endDate=t.target.value}}
            />
          </div>
          <button class="btn" @click=${this._fetch} ?disabled=${this._loading}>
            ${this._loading?"Loading...":"Load"}
          </button>
        </div>

        ${this._error?L`<div class="no-data">Error: ${this._error}</div>`:""}
        ${this._records.length>0?this._renderTable():""}
        ${this._loading||0!==this._records.length||this._error?"":L`<div class="no-data">
              Select a date range and click Load
            </div>`}
      </div>
    `}_renderTable(){const t=Math.ceil(this._totalCount/$t),e=Math.floor(this._offset/$t)+1;return L`
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>Purchased kWh</th>
              <th>Cost SEK</th>
              <th>Sold kWh</th>
              <th>Profit SEK</th>
              <th>Own Use kWh</th>
              <th>Saved SEK</th>
              <th>Price Level</th>
              <th>Synced</th>
              <th>Enriched</th>
            </tr>
          </thead>
          <tbody>
            ${this._records.map(t=>L`
                <tr>
                  <td>${this._formatTs(t.timestamp)}</td>
                  <td>${t.purchased.toFixed(3)}</td>
                  <td>${t.purchased_cost.toFixed(2)}</td>
                  <td>${t.production_sold.toFixed(3)}</td>
                  <td>${t.production_sold_profit.toFixed(2)}</td>
                  <td>${t.production_own_use.toFixed(3)}</td>
                  <td>${t.production_own_use_profit.toFixed(2)}</td>
                  <td>${t.price_level||"-"}</td>
                  <td>${t.synced?"Yes":"No"}</td>
                  <td>${t.sensor_enriched?"Yes":"No"}</td>
                </tr>
              `)}
          </tbody>
        </table>
      </div>
      <div class="pagination">
        <button
          class="btn"
          ?disabled=${0===this._offset||this._loading}
          @click=${this._prevPage}
        >
          Prev
        </button>
        <span>Page ${e} of ${t} (${this._totalCount} records)</span>
        <button
          class="btn"
          ?disabled=${this._offset+$t>=this._totalCount||this._loading}
          @click=${this._nextPage}
        >
          Next
        </button>
      </div>
    `}async _fetch(){if(this.hass&&this.entryId&&this._startDate&&this._endDate){this._loading=!0,this._error="";try{const t=`${this._startDate}T00:00:00`,e=new Date(this._endDate);e.setDate(e.getDate()+1);const s=`${e.toISOString().substring(0,10)}T00:00:00`,i=await this.hass.callWS({type:"my_solar_cells/get_hourly_energy",entry_id:this.entryId,start_date:t,end_date:s,offset:this._offset,limit:$t});this._records=i.records,this._totalCount=i.total_count}catch(t){this._error=t.message||"Failed to fetch data",this._records=[],this._totalCount=0}this._loading=!1}}_prevPage(){this._offset=Math.max(0,this._offset-$t),this._fetch()}_nextPage(){this._offset+=$t,this._fetch()}_formatTs(t){try{return t.replace("T"," ").substring(0,19)}catch{return t}}};xt.styles=[gt,yt],t([pt({attribute:!1})],xt.prototype,"hass",void 0),t([pt()],xt.prototype,"entryId",void 0),t([ut()],xt.prototype,"_startDate",void 0),t([ut()],xt.prototype,"_endDate",void 0),t([ut()],xt.prototype,"_records",void 0),t([ut()],xt.prototype,"_totalCount",void 0),t([ut()],xt.prototype,"_offset",void 0),t([ut()],xt.prototype,"_loading",void 0),t([ut()],xt.prototype,"_error",void 0),xt=t([lt("hourly-energy-view")],xt);let wt=class extends nt{constructor(){super(...arguments),this.entryId="",this._sensors=[],this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._sensors.length&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_sensor_config",entry_id:this.entryId});this._sensors=t.sensors}catch(t){this._error=t.message||"Failed to fetch sensor config"}this._loading=!1}}render(){if(this._loading)return L`<div class="loading">Loading sensor configuration...</div>`;if(this._error)return L`<div class="no-data">Error: ${this._error}</div>`;this._sensors.filter(t=>t.entity_id);const t=this._sensors.filter(t=>!t.entity_id);return L`
      <div class="info-box">
        Only the <strong>production</strong> sensor is required — it is used to
        calculate <strong>production_own_use</strong> (total production minus
        grid export). All other data (grid export, grid import) comes from the
        <strong>Tibber API</strong> by default. You can optionally override
        grid export/import with HA sensors for higher accuracy, and add battery
        sensors if you have a battery. Sensors are configured in the
        integration setup flow.
      </div>

      <div class="card">
        <h3>Sensor Configuration</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Status</th>
                <th>Role</th>
                <th>Description</th>
                <th>Entity ID</th>
                <th>Current State</th>
                <th>Last Stored Reading</th>
              </tr>
            </thead>
            <tbody>
              ${this._sensors.map(t=>this._renderRow(t))}
            </tbody>
          </table>
        </div>
      </div>

      ${t.some(t=>"production"===t.role)?L`
            <div class="card">
              <h3>Required Sensor Missing</h3>
              <p style="color: var(--error-color, #f44336); font-size: 0.9em;">
                The <strong>production</strong> sensor is not configured.
                Without it, <strong>production_own_use</strong> cannot be
                calculated. Please configure it in the integration setup flow.
              </p>
            </div>
          `:j}

      <div style="margin-top: 12px;">
        <button class="btn" @click=${this._fetchData} ?disabled=${this._loading}>
          Refresh
        </button>
      </div>
    `}_isRequired(t){return"production"===t}_getFallbackLabel(t){return"grid_export"===t||"grid_import"===t?"Using Tibber API":"Not configured"}_renderRow(t){const e=!!t.entity_id,s=this._isRequired(t.role);return L`
      <tr>
        <td>
          <span class="status-dot ${e?"configured":s?"missing":"optional"}"></span>
        </td>
        <td>
          <strong>${t.role}</strong>
          ${s?L`<span class="required-badge">Required</span>`:L`<span class="optional-badge">Optional</span>`}
        </td>
        <td>${t.description}</td>
        <td>
          ${e?L`<span class="entity-id">${t.entity_id}</span>`:L`<span class="fallback-label">${this._getFallbackLabel(t.role)}</span>`}
        </td>
        <td>
          ${null!=t.current_state?t.current_state:L`<span class="not-configured">-</span>`}
        </td>
        <td>
          ${null!=t.last_stored_reading?t.last_stored_reading.toFixed(3):L`<span class="not-configured">-</span>`}
        </td>
      </tr>
    `}};wt.styles=[gt,yt,o`
      .status-dot {
        display: inline-block;
        width: 10px;
        height: 10px;
        border-radius: 50%;
        margin-right: 8px;
      }
      .status-dot.configured {
        background: var(--success-color, #4caf50);
      }
      .status-dot.missing {
        background: var(--error-color, #f44336);
      }
      .status-dot.optional {
        background: var(--warning-color, #ff9800);
      }
      .fallback-label {
        color: var(--secondary-text-color);
        font-size: 0.85em;
        font-style: italic;
      }
      .required-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--error-color, #f44336);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .optional-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--secondary-text-color);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .entity-id {
        font-family: monospace;
        font-size: 0.85em;
        color: var(--primary-text-color);
      }
      .not-configured {
        color: var(--secondary-text-color);
        font-style: italic;
      }
      .info-box {
        background: var(--primary-background-color);
        border-radius: 8px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.9em;
        color: var(--secondary-text-color);
        line-height: 1.5;
      }
    `],t([pt({attribute:!1})],wt.prototype,"hass",void 0),t([pt()],wt.prototype,"entryId",void 0),t([ut()],wt.prototype,"_sensors",void 0),t([ut()],wt.prototype,"_loading",void 0),t([ut()],wt.prototype,"_error",void 0),wt=t([lt("sensors-view")],wt);const At={tax_reduction:.6,grid_compensation:.078,transfer_fee:.3,energy_tax:.49,installed_kw:10};let St=class extends nt{constructor(){super(...arguments),this.entryId="",this._params={},this._loading=!1,this._fetched=!1,this._error="",this._editingYear=null,this._editValues={...At},this._newYear="",this._saving=!1,this._minYear=0,this._maxYear=0}updated(t){(t.has("hass")||t.has("entryId"))&&this.hass&&this.entryId&&!this._loading&&!this._fetched&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_yearly_params",entry_id:this.entryId});this._params=t.yearly_params,t.first_timestamp&&(this._minYear=new Date(t.first_timestamp).getFullYear()),t.last_timestamp&&(this._maxYear=new Date(t.last_timestamp).getFullYear())}catch(t){this._error=t.message||"Failed to fetch yearly params"}this._loading=!1,this._fetched=!0}}render(){if(this._loading)return L`<div class="loading">Loading yearly parameters...</div>`;if(this._error)return L`<div class="no-data">Error: ${this._error}</div>`;const t=Object.keys(this._params).sort();return L`
      <div class="card">
        <h3>Yearly Financial Parameters</h3>

        <div class="add-year-row">
          <div class="input-group">
            <label>Add Year</label>
            <select
              .value=${this._newYear}
              @change=${t=>this._newYear=t.target.value}
            >
              <option value="">Select year...</option>
              ${this._getAvailableYears().map(t=>L`<option value=${t}>${t}</option>`)}
            </select>
          </div>
          <button class="btn" @click=${this._addYear}>Add</button>
        </div>

        ${0===t.length?L`<div class="no-data">
              No yearly parameters configured yet. Add a year above.
            </div>`:L`
              <div class="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Year</th>
                      <th>Tax Reduction</th>
                      <th>Grid Comp.</th>
                      <th>Transfer Fee</th>
                      <th>Energy Tax</th>
                      <th>Installed kW</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${t.map(t=>{const e=this._params[t],s=this._editingYear===t;return L`
                        <tr
                          class="clickable ${s?"selected":""}"
                          @click=${()=>this._startEdit(t)}
                        >
                          <td>${t}</td>
                          <td>${this._fmt(e.tax_reduction)}</td>
                          <td>${this._fmt(e.grid_compensation)}</td>
                          <td>${this._fmt(e.transfer_fee)}</td>
                          <td>${this._fmt(e.energy_tax)}</td>
                          <td>${null!=e.installed_kw?e.installed_kw:"-"}</td>
                        </tr>
                      `})}
                  </tbody>
                </table>
              </div>
            `}
      </div>

      ${null!=this._editingYear?this._renderEditForm():j}
    `}_renderEditForm(){const t=this._editValues;return L`
      <div class="card">
        <h3>Edit ${this._editingYear}</h3>
        <div class="edit-form">
          <div class="input-group">
            <label>Tax Reduction (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.tax_reduction??"")}
              @input=${t=>this._editValues={...this._editValues,tax_reduction:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Grid Compensation (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.grid_compensation??"")}
              @input=${t=>this._editValues={...this._editValues,grid_compensation:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Transfer Fee (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.transfer_fee??"")}
              @input=${t=>this._editValues={...this._editValues,transfer_fee:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Energy Tax (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.energy_tax??"")}
              @input=${t=>this._editValues={...this._editValues,energy_tax:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Installed kW</label>
            <input
              type="number"
              step="0.01"
              .value=${String(t.installed_kw??"")}
              @input=${t=>this._editValues={...this._editValues,installed_kw:parseFloat(t.target.value)}}
            />
          </div>
        </div>
        <div class="form-actions">
          <button class="btn" ?disabled=${this._saving} @click=${this._save}>
            ${this._saving?"Saving...":"Save"}
          </button>
          <button class="btn btn-secondary" @click=${this._cancelEdit}>
            Cancel
          </button>
          <button class="btn btn-danger" @click=${this._delete}>Delete</button>
        </div>
      </div>
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_startEdit(t){this._editingYear=t;const e=this._params[t]||{};this._editValues={tax_reduction:e.tax_reduction??At.tax_reduction,grid_compensation:e.grid_compensation??At.grid_compensation,transfer_fee:e.transfer_fee??At.transfer_fee,energy_tax:e.energy_tax??At.energy_tax,installed_kw:e.installed_kw??At.installed_kw}}_cancelEdit(){this._editingYear=null}_getAvailableYears(){const t=[];for(let e=this._minYear;e<=this._maxYear;e++)this._params[String(e)]||t.push(e);return t}_addYear(){const t=parseInt(this._newYear,10);if(isNaN(t))return;const e=String(t);if(this._params[e])return this._startEdit(e),void(this._newYear="");const s=Object.keys(this._params).filter(t=>t<e).sort(),i=s.length>0?this._params[s[s.length-1]]:null;this._editValues=i?{...i}:{...At},this._editingYear=e,this._newYear=""}async _save(){if(null!=this._editingYear&&this.hass&&this.entryId){this._saving=!0;try{await this.hass.callWS({type:"my_solar_cells/set_yearly_params",entry_id:this.entryId,year:parseInt(this._editingYear,10),tax_reduction:this._editValues.tax_reduction??0,grid_compensation:this._editValues.grid_compensation??0,transfer_fee:this._editValues.transfer_fee??0,energy_tax:this._editValues.energy_tax??0,installed_kw:this._editValues.installed_kw??0}),this._editingYear=null,await this._fetchData()}catch(t){this._error=t.message||"Failed to save"}this._saving=!1}}async _delete(){if(null!=this._editingYear&&this.hass&&this.entryId&&confirm(`Delete parameters for ${this._editingYear}?`)){this._saving=!0;try{await this.hass.callWS({type:"my_solar_cells/delete_yearly_params",entry_id:this.entryId,year:parseInt(this._editingYear,10)}),this._editingYear=null,await this._fetchData()}catch(t){this._error=t.message||"Failed to delete"}this._saving=!1}}};St.styles=[gt,yt,o`
      .edit-form {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 12px;
        margin-top: 12px;
      }

      .edit-form .input-group input {
        width: 100%;
        box-sizing: border-box;
      }

      .form-actions {
        display: flex;
        gap: 8px;
        margin-top: 16px;
        align-items: center;
      }

      .btn-danger {
        background: var(--error-color, #db4437) !important;
      }

      .btn-secondary {
        background: var(--secondary-text-color) !important;
      }

      .add-year-row {
        display: flex;
        gap: 8px;
        align-items: flex-end;
        margin-bottom: 16px;
      }

      .add-year-row .input-group select {
        width: 100px;
        padding: 6px 8px;
        border: 1px solid var(--divider-color, #e0e0e0);
        border-radius: 4px;
        background: var(--card-background-color, #fff);
        color: var(--primary-text-color);
        font-size: 14px;
      }

      tr.clickable {
        cursor: pointer;
      }

      tr.clickable:hover td {
        background: var(--primary-background-color);
      }

      tr.selected td {
        background: color-mix(in srgb, var(--primary-color) 12%, transparent);
      }
    `],t([pt({attribute:!1})],St.prototype,"hass",void 0),t([pt()],St.prototype,"entryId",void 0),t([ut()],St.prototype,"_params",void 0),t([ut()],St.prototype,"_loading",void 0),t([ut()],St.prototype,"_fetched",void 0),t([ut()],St.prototype,"_error",void 0),t([ut()],St.prototype,"_editingYear",void 0),t([ut()],St.prototype,"_editValues",void 0),t([ut()],St.prototype,"_newYear",void 0),t([ut()],St.prototype,"_saving",void 0),t([ut()],St.prototype,"_minYear",void 0),t([ut()],St.prototype,"_maxYear",void 0),St=t([lt("yearly-params-view")],St);let kt=class extends nt{constructor(){super(...arguments),this._activeTab="overview"}get _entryId(){return this.panel?.config?.entry_id||""}render(){return L`
      <div class="content">
        <div class="header">
          <h1>Solar Data Browser</h1>
        </div>
        <div class="tabs">
          <button
            class="tab ${"overview"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="overview"}
          >
            Overview
          </button>
          <button
            class="tab ${"hourly"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="hourly"}
          >
            Hourly Energy
          </button>
          <button
            class="tab ${"sensors"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="sensors"}
          >
            Sensors
          </button>
          <button
            class="tab ${"params"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="params"}
          >
            Yearly Params
          </button>
        </div>
        ${this._renderActiveTab()}
      </div>
    `}_renderActiveTab(){switch(this._activeTab){case"overview":return L`
          <overview-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></overview-view>
        `;case"hourly":return L`
          <hourly-energy-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></hourly-energy-view>
        `;case"sensors":return L`
          <sensors-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></sensors-view>
        `;case"params":return L`
          <yearly-params-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></yearly-params-view>
        `}}};kt.styles=[_t,o`
      .content {
        max-width: 1200px;
        margin: 0 auto;
      }
    `],t([pt({attribute:!1})],kt.prototype,"hass",void 0),t([pt({attribute:!1})],kt.prototype,"narrow",void 0),t([pt({attribute:!1})],kt.prototype,"route",void 0),t([pt({attribute:!1})],kt.prototype,"panel",void 0),t([ut()],kt.prototype,"_activeTab",void 0),kt=t([lt("my-solar-cells-panel")],kt);export{kt as MySolarCellsPanel};
